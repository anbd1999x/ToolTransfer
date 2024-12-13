using System.Data.SqlClient;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Dapper;

namespace ToolTransfer
{
    public partial class Form1 : Form
    {
        private AmazonS3Client _s3Client { get; set; }
        private bool isConnectSQL { get; set; }
        private bool isConnectaws3 { get; set; }

        public Form1()
        {
            InitializeComponent();
            txtConnectString.Text = "Data Source=103.72.98.97;Initial Catalog=LOYALTY_DEV_20241018;User ID=sa;Password=Loyalty@123;";
            txtAws3Url.Text = "https://s3-hcmc02.higiocloud.vn";
            TxtAccessKey.Text = "ZWLSY7YABWDQFR0XSGDI";
            TxtSecretKey.Text = "0LNs9uQ9IBJZMLsy7x1ltXUVdgE0VmJoJ5MTVRwL";
            txtBucket.Text = "loyatly";
            txtFolder.Text = "dev";
            isConnectSQL = false;
            isConnectaws3 = false;
        }
        private async void TransferData(object sender, EventArgs e)
        {
            try
            {
                if (!isConnectaws3 || !isConnectSQL)
                {
                    MessageBox.Show("Vui lòng kiểm tra kết nối tới database và Aws3 trước", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                btnTransfer.Enabled = false;
                // Thực thi các công việc nặng trong background thread
                var tableQuerys = GetCheckedTablesAndColumns(treeViewDatabase.Nodes);
                List<TableUpdate> tableUpdates = new List<TableUpdate>();
                // lấy dữ liệu trong DB
                await Task.Run((Action)(() =>
                {

                    foreach (var table in tableQuerys)
                    {
                        if (table.Columns.Count > 0)
                        {
                            string query = string.Empty;
                            string qureytable = string.Empty;

                            qureytable = string.Join(",", Enumerable.Select<TableColumn, string>(table.Columns, (Func<TableColumn, string>)(c => c.ColumnName)));
                            query = $"select id,{qureytable} from [{table.TableName}]";

                            // Cấu hình kết nối tới database
                            string connectionString = txtConnectString.Text; // Thay thế bằng chuỗi kết nối của bạn
                            using (var connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                var result = connection.Query(query).ToList();
                                // Danh sách các bảng cần update
                                var tableUpdate = new TableUpdate
                                {
                                    TableName = table.TableName,
                                    RowColum = new List<TableRowUpdate>()
                                };

                                foreach (var item in result)
                                {
                                    var dictItem = (IDictionary<string, object>)item;
                                    // Tạo một đối tượng TableUpdate mới

                                    // Tạo đối tượng TableColumnUpdate cho mỗi dòng dữ liệu
                                    var tableColumnUpdate = new TableRowUpdate
                                    {
                                        id = dictItem["id"].ToString(), // lấy id từ dictItem
                                        Columnames = new List<ColumsUpdate>()
                                    };
                                    foreach (var column in table.Columns)
                                    {

                                        if (dictItem.ContainsKey((string)column.ColumnName))
                                        {
                                            var value = dictItem[column.ColumnName]?.ToString() ?? string.Empty;
                                            // Tạo đối tượng ColumsUpdate
                                            var columnsUpdate = new ColumsUpdate
                                            {
                                                ColumnName = column.ColumnName,
                                                value = value
                                            };
                                            tableColumnUpdate.Columnames.Add(columnsUpdate);

                                            // Đảm bảo việc cập nhật RichTextBox được thực thi trên UI thread
                                            Invoke(new Action(() =>
                                            {
                                                UpdateRichTextBox($"{column.ColumnName}: {value}", Color.Green);
                                            }));
                                        }
                                    }

                                    // Thêm vào danh sách TableUpdate
                                    tableUpdate.RowColum.Add(tableColumnUpdate);
                                }
                                tableUpdates.Add(tableUpdate);

                            }

                        }

                    }


                }));


                // đẩy lên AWS3
                await Task.Run((Action)(async () =>
                {
                    foreach (var item in tableUpdates)
                    {
                        foreach (var colum in item.RowColum)
                        {

                            foreach (var column in colum.Columnames)
                            {
                                if (column.value != null && column.value.Contains("https") && !column.value.Contains("higiocloud.vn"))
                                {
                                    var newLink = await UploadFileToS3Async(item.TableName, column.value);
                                    column.newValue = newLink;
                                }
                            }
                            await UpdateDataBaseForNewLink(item.TableName, colum);
                        }
                        AppendTextToRichTextBox($"Hoàn thành cập nhật link cho bảng : {item.TableName}", Color.Green);
                    }

                }));
                AppendTextToRichTextBox($"Hoàn thành toàn bộ cập nhật", Color.Green);
                btnTransfer.Enabled = true;
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox($"Lỗi khi cập nhật link mới cho bảng lỗi: {ex.Message}", Color.Red);

            }

        }

        /// <summary>
        /// UploadFileToS3Async
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDataBaseForNewLink(string table, TableRowUpdate RowColum)
        {
            try
            {
                string query = $"UPDATE [{table}] SET ";
                var parameters = new DynamicParameters();
                int index = 0;
                bool isHasnewValue = false;
                foreach (var item in RowColum.Columnames)
                {
                    if (!string.IsNullOrEmpty(item.newValue))
                    {
                        isHasnewValue = true;
                        string paramName = $"@param{index}";
                        if (index == 0)
                        {
                            query += $"{item.ColumnName} = {paramName}";
                        }
                        else
                        {
                            query += $", {item.ColumnName} = {paramName}";
                        }
                        parameters.Add(paramName, item.newValue);
                        index++;
                    }
                }
                if(!isHasnewValue)
                {
                    return false;
                }
                query += " WHERE id = @id";
                parameters.Add("@id", RowColum.id);
                using (var connection = new SqlConnection(txtConnectString.Text))
                {
                    await connection.OpenAsync();
                    int rowsAffected = await connection.ExecuteAsync(query, parameters);
                    if (rowsAffected > 0)
                    {
                        Invoke(new Action(() =>
                        {
                            UpdateRichTextBox($"Hoàn thành cập nhật link mới cho bảng {table} where id = {RowColum.id} thành công", Color.Green);
                        }));
                        return true;
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            UpdateRichTextBox($"Hoàn thành cập nhật link mới cho bảng {table} where id = {RowColum.id} Thất bại", Color.Green);
                        }));

                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox($"Lỗi khi cập nhật link mới cho bảng lỗi: {ex.Message}", Color.Red);
                return false;
            }
        }

        /// <summary>
        /// UploadFileToS3Async
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public async Task<string> UploadFileToS3Async(string table, string fileUrl)
        {
            try
            {
                // Tải dữ liệu từ URL
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(fileUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error fetching file from URL: {response.StatusCode}");
                        return string.Empty;
                    }
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {

                        AmazonS3Config config = new AmazonS3Config();
                        config.ServiceURL = txtAws3Url.Text;
                        _s3Client = new AmazonS3Client(TxtAccessKey.Text, TxtSecretKey.Text, config);
                        string Key = $"{txtFolder.Text.ToLower()}/{table}/{Path.GetFileName(fileUrl)}";
                        // Tạo yêu cầu tải tệp lên S3
                        var request = new PutObjectRequest
                        {
                            BucketName = txtBucket.Text,
                            Key = Key,
                            InputStream = stream,
                            CannedACL = S3CannedACL.PublicRead
                        };
                        // Tải lên S3
                        var result = await _s3Client.PutObjectAsync(request);
                        if(result.HttpStatusCode == HttpStatusCode.OK)
                        {
                            var path = $"{txtAws3Url.Text}/{txtBucket.Text}/{Key}";
                            // Đảm bảo việc cập nhật RichTextBox được thực thi trên UI thread
                            Invoke(new Action(() =>
                            {
                                UpdateRichTextBox($"Hoàn thành đẩy file từ  oldPath : {fileUrl} sang newpath: {path} File uploaded successfully, HTTP status: {result.HttpStatusCode}", Color.Green);
                            }));
                            return path;

                        }
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                AppendTextToRichTextBox($"Hoàn thành đẩy file từ  oldPath : {fileUrl} lỗi : {ex.Message},{ex.InnerException} ", Color.Red);
                return string.Empty;
            }


        }

        /// <summary>
        /// GetCheckedTablesAndColumns
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private List<Table> GetCheckedTablesAndColumns(TreeNodeCollection nodes)
        {
            var tables = new List<Table>();

            foreach (TreeNode node in nodes)
            {
                // Nếu là bảng (node cha)
                if (node.Nodes.Count > 0)
                {
                    var table = new Table
                    {
                        TableName = node.Text, // Lấy tên bảng
                        Columns = new List<TableColumn>()
                    };

                    // Duyệt qua các cột của bảng
                    foreach (TreeNode childNode in node.Nodes)
                    {
                        if (childNode.Checked)
                        {
                            table.Columns.Add(new TableColumn
                            {
                                ColumnName = ExtractName(childNode.Text), // Lấy tên cột
                                IsChecked = true
                            });
                        }
                    }

                    // Chỉ thêm bảng nếu có ít nhất một cột được check
                    if (node.Checked || table.Columns.Any())
                    {
                        tables.Add(table);
                    }
                }
                else if (node.Checked)
                {
                    // Trường hợp node cha được check nhưng không có node con
                    tables.Add(new Table
                    {
                        TableName = node.Text,
                        Columns = new List<TableColumn>() // Không có cột nào
                    });
                }
            }

            return tables;
        }
        /// <summary>
        /// ExtractName
        /// </summary>
        /// <param name="fullText"></param>
        /// <returns></returns>
        private string ExtractName(string fullText)
        {
            // Tìm vị trí của ký tự '('
            int index = fullText.IndexOf('(');

            // Nếu không có '(', trả về toàn bộ chuỗi
            if (index == -1)
                return fullText;

            // Trả về phần trước dấu '('
            return fullText.Substring(0, index).Trim();
        }
        /// <summary>
        /// LoadDatabaseSchema
        /// </summary>
        private void LoadDatabaseSchema()
        {
            using (var connection = new SqlConnection(txtConnectString.Text))
            {
                connection.Open();
                isConnectSQL = true;
                var query = @"
                    SELECT 
                       TABLE_NAME AS TableName,
                       COLUMN_NAME AS ColumnName,
                       DATA_TYPE AS DataType,
                       IS_NULLABLE AS IsNullable,
                       CHARACTER_MAXIMUM_LENGTH AS MaxLength
                   FROM INFORMATION_SCHEMA.COLUMNS
                   WHERE TABLE_SCHEMA = 'dbo'
                   ORDER BY TABLE_NAME, ORDINAL_POSITION;";


                var schema = connection.Query<TableColumn>(query).ToList();


                // Kiểm tra xem có dữ liệu trả về không
                if (schema.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu bảng/cột.");
                    return;
                }

                var tables = schema
                        .GroupBy(col => col.TableName)
                        .Select(group => new Table
                        {
                            TableName = group.Key,
                            Columns = group.ToList()
                        })
                        .ToList();

                // Thêm dữ liệu vào TreeView
                foreach (var tableGroup in schema.GroupBy(s => s.TableName))
                {

                    // Thêm nút bảng (parent node) với CheckBox
                    var tableNode = new TreeNode(tableGroup.Key) { Checked = false };
                    treeViewDatabase.Nodes.Add(tableNode);

                    // Thêm các cột của bảng (child nodes) với thông tin chi tiết
                    foreach (var column in tableGroup)
                    {
                        string columnDetails = $"{column.ColumnName} ({column.DataType}, Nullable: {column.IsNullable}, MaxLength: {column.MaxLength?.ToString() ?? "N/A"})";
                        var columnNode = new TreeNode(columnDetails) { Checked = false };
                        tableNode.Nodes.Add(columnNode);
                    }
                }
            }

        }
        /// <summary>
        /// treeViewDatabase_AfterCheck
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewDatabase_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //if (e.Node.Parent == null)
            //{
            //    // Nếu là bảng
            //    MessageBox.Show($"Bạn đã {(e.Node.Checked ? "chọn" : "bỏ chọn")} bảng: {e.Node.Text}");
            //}
            //else
            //{
            //    // Nếu là cột
            //    MessageBox.Show($"Bạn đã {(e.Node.Checked ? "chọn" : "bỏ chọn")} cột: {e.Node.Text} của bảng: {e.Node.Parent.Text}");
            //}

        }
        /// <summary>
        /// treeViewDatabase_AfterSelect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeViewDatabase_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Xử lý khi click vào một node
            if (e.Node.Parent == null)
            {
                e.Node.Checked = !e.Node.Checked;

                // Nếu là bảng
                //MessageBox.Show($"Bạn đã chọn bảng: {e.Node.Text}");
            }
            else
            {
                e.Node.Checked = !e.Node.Checked;

                // Nếu là cột
                //MessageBox.Show($"Bạn đã chọn cột: {e.Node.Text} của bảng: {e.Node.Parent.Text}");
            }

        }

        // Model đại diện cho thông tin cột

        private void Checkconnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtConnectString.Text))
            {
                MessageBox.Show("Chưa cấu hình connectString", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string connectionString = txtConnectString.Text; // Thay bằng chuỗi kết nối của bạn
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Thử mở kết nối
                    LoadDatabaseSchema();
                    // Nếu kết nối thành công
                    MessageBox.Show("Kết nối đến cơ sở dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateRichTextBox("Kết nối đến cơ sở dữ liệu thành công", Color.Green);

                }

            }
            catch (Exception ex)
            {
                // Nếu xảy ra lỗi
                MessageBox.Show($"Không thể kết nối đến cơ sở dữ liệu. Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateRichTextBox($"Không thể kết nối đến cơ sở dữ liệu. Lỗi: {ex.Message}", Color.Green);

            }
        }
        /// <summary>
        /// TestConenctAws3_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TestConenctAws3_Click(object sender, EventArgs e)
        {
            try
            {
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = txtAws3Url.Text;
                config.Timeout = TimeSpan.FromSeconds(15);
                _s3Client = new AmazonS3Client(TxtAccessKey.Text, TxtSecretKey.Text, config);
                var res = await _s3Client.ListBucketsAsync();
                if (res != null)
                {
                    isConnectaws3 = true;

                    var listData = res.Buckets;
                    var bucket = listData.Any(x => x.BucketName.Trim() == txtBucket.Text?.Trim());
                    if (!bucket)
                    {
                        await _s3Client.PutBucketAsync(txtBucket.Text);

                    }
                    MessageBox.Show($"Kết nối đến Aws3 thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    AppendTextToRichTextBox("Kết nối đến Aws3 thành công", Color.Green);
                }
                return;
            }
            catch (Exception ex)
            {
                isConnectaws3 = false;
                MessageBox.Show($"Không thể kết nối đến Aws3. Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendTextToRichTextBox($"Không thể kết nối đến Aws3.Lỗi: {ex.Message}", Color.Red);
            }
        }
        /// <summary>
        /// UpdateRichTextBox
        /// </summary>
        /// <param name="text"></param>
        private void UpdateRichTextBox(string text, Color textColor)
        {
            if (richTextBox1.InvokeRequired)
            {
                // Nếu không phải trên UI thread, gọi Invoke để thực hiện trên UI thread
                richTextBox1.Invoke(new Action<string, Color>(AppendTextToRichTextBox), text);
            }
            else
            {
                // Nếu đã ở trên UI thread, gọi trực tiếp
                AppendTextToRichTextBox(text, textColor);
            }
        }

        private void AppendTextToRichTextBox(string text, Color textColor)
        {
            try
            {
                // Đặt SelectionStart tới cuối để thêm văn bản mới
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectionLength = 0;

                // Đặt màu sắc cho văn bản
                richTextBox1.SelectionColor = textColor;

                // Thêm văn bản vào RichTextBox
                richTextBox1.AppendText(text + Environment.NewLine);

                // Đặt lại màu sắc về mặc định (nếu cần)
                richTextBox1.SelectionColor = richTextBox1.ForeColor;

                // Cuộn xuống cuối RichTextBox
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần
            }
        }


    }

    /// <summary>
     /// TableColumn
     /// </summary>
    public class TableColumn
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; } // "YES" hoặc "NO"
        public int? MaxLength { get; set; }    // Độ dài tối đa (NULL nếu không phải kiểu ký tự)
        public bool IsChecked { get; set; } // Trạng thái Checked

    }

    // Model đại diện cho thông tin bảng
    public class Table
    {
        public string TableName { get; set; }
        public List<TableColumn> Columns { get; set; }
    }
    public class TableUpdate
    {
        public string TableName { get; set; }
        public List<TableRowUpdate> RowColum { get; set; }
    }
    public class TableRowUpdate
    {
        public string id { get; set; }
        public List<ColumsUpdate> Columnames { get; set; }
    }
    public class ColumsUpdate
    {
        public string ColumnName { get; set; }
        public string value { get; set; }
        public string newValue { get; set; }
    }

}
