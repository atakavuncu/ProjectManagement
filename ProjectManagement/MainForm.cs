using ProjectManagement.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectManagement
{
    public partial class MainForm : Form
    {
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;
        private SqlConnection connection;
        private string connectionString;
        private string selectedProjectName = "";
        private List<Classes.Task> tasks = new List<Classes.Task>();
        private List<Milestone> milestones = new List<Milestone>();

        public MainForm()
        {
            InitializeComponent();
            buttonProjeler.BackColor = Color.FromArgb(8, 133, 93);
            tabControl.TabPages.Remove(tabPageKullanicilar);
            tabControl.TabPages.Remove(tabPageProjeEkle);
            tabControl.TabPages.Remove(tabPageMilestone);
            tabControl.TabPages.Remove(tabPageTaskEkle);
            tabControl.TabPages.Remove(tabPageProjeDuzenle);
            tabControl.TabPages.Remove(tabPageKisiEkle);
            tabControl.TabPages.Remove(tabPageKisiDuzenle);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            connection.Close();
            dataAdapter.Dispose();
            dataTable.Dispose();
            Application.Exit();
        }

        private void buttonProjeler_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageProjeler))
                tabControl.TabPages.Add(tabPageProjeler);
            tabControl.SelectedTab = tabPageProjeler;
            buttonProjeler.BackColor = Color.FromArgb(8, 133, 93);
            buttonKullanicilar.BackColor = Color.FromArgb(7, 123, 88);
        }

        private void buttonKullanicilar_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageKullanicilar))
                tabControl.TabPages.Add(tabPageKullanicilar);
            tabControl.SelectedTab = tabPageKullanicilar;
            buttonKullanicilar.BackColor = Color.FromArgb(8, 133, 93);
            buttonProjeler.BackColor = Color.FromArgb(7, 123, 88);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            connectionString = "Data Source=atakavuncu\\SQLEXPRESS;Initial Catalog=ProjectManagement;Integrated Security=True";
            connection = new SqlConnection(connectionString);
            dataAdapter = createDataAdapter(connection, null);

            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            dataGridViewProjeler.DataSource = dataTable;
            dataGridViewProjeEkle.DataSource = dataTable;

            LoadProjects(connectionString);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Person";

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();

                    dataAdapter.Fill(dataTable);

                    dataGridViewCalisanlar.DataSource = dataTable;
                }
            }
        }

        private SqlDataAdapter createDataAdapter(SqlConnection connection, int? selectedProjectID)
        {
            string selectQuery = "SELECT ProjectTable.*, Person.FirstName + ' ' + Person.LastName AS ProjectManagerName, Status.Status, MonetaryReturnType.MonetaryReturnType, ProjectType.ProjectType " +
                "FROM ProjectTable " +
                "INNER JOIN Person ON ProjectTable.ProjectManagerID = Person.ID " +
                "INNER JOIN Status ON ProjectTable.StatusID = Status.ID " +
                "INNER JOIN MonetaryReturnType ON ProjectTable.MonetaryReturnTypeID = MonetaryReturnType.ID " +
                "INNER JOIN ProjectType ON ProjectTable.ProjectTypeID = ProjectType.ID";

            if (selectedProjectID.HasValue)
            {
                selectQuery += $" WHERE ProjectTable.ProjectID = {selectedProjectID}";
            }

            dataAdapter = new SqlDataAdapter(selectQuery, connection);

            int lastColumnIndex = dataAdapter.SelectCommand.Parameters.Count - 1;

            if (lastColumnIndex >= 0 && lastColumnIndex != 2)
            {
                SqlParameter parameter = dataAdapter.SelectCommand.Parameters[2];
                dataAdapter.SelectCommand.Parameters.RemoveAt(2);
                dataAdapter.SelectCommand.Parameters.Insert(lastColumnIndex - 3, parameter);
            }
            if (lastColumnIndex >= 0 && lastColumnIndex != 11)
            {
                SqlParameter parameter = dataAdapter.SelectCommand.Parameters[11];
                dataAdapter.SelectCommand.Parameters.RemoveAt(11);
                dataAdapter.SelectCommand.Parameters.Insert(lastColumnIndex - 2, parameter);
            }
            if (lastColumnIndex >= 0 && lastColumnIndex != 13)
            {
                SqlParameter parameter = dataAdapter.SelectCommand.Parameters[13];
                dataAdapter.SelectCommand.Parameters.RemoveAt(13);
                dataAdapter.SelectCommand.Parameters.Insert(lastColumnIndex - 1, parameter);
            }
            if (lastColumnIndex >= 0 && lastColumnIndex != 15)
            {
                SqlParameter parameter = dataAdapter.SelectCommand.Parameters[15];
                dataAdapter.SelectCommand.Parameters.RemoveAt(15);
                dataAdapter.SelectCommand.Parameters.Insert(lastColumnIndex, parameter);
            }
            return dataAdapter;
        }



        private void dataGridViewProjeler_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || Convert.IsDBNull(e.Value))
            {
                e.Value = "---";
                e.FormattingApplied = true;
            }

        }

        private void LoadProjects(string connectionString)
        {
            treeViewProjeler.Nodes.Clear();
            using (SqlConnection connectionProjects = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM ProjectTable";

                connectionProjects.Open();
                using (SqlCommand command = new SqlCommand(query, connectionProjects))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string projectName = reader["ProjectName"].ToString();
                        int projectID = Convert.ToInt32(reader["ProjectID"]);

                        TreeNode projectNode = treeViewProjeler.Nodes.Add(projectName);

                        LoadMilestones(projectNode, projectID, connectionString);
                    }

                    reader.Close();
                }
            }
        }

        private void LoadMilestones(TreeNode projectNode, int projectID, string connectionString)
        {
            using (SqlConnection connectionMilestones = new SqlConnection(connectionString))
            {
                string query = $"SELECT * FROM Milestone WHERE ProjectID = {projectID}";

                connectionMilestones.Open();
                using (SqlCommand command = new SqlCommand(query, connectionMilestones))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string milestoneName = reader["MilestoneName"].ToString();
                        int milestoneID = Convert.ToInt32(reader["ID"]);

                        TreeNode milestoneNode = projectNode.Nodes.Add(milestoneName);

                        LoadTasks(milestoneNode, projectID, milestoneID, connectionString);
                    }

                    reader.Close();
                }
            }
        }

        private void LoadTasks(TreeNode milestoneNode, int projectID, int milestoneID, string connectionString)
        {
            using (SqlConnection connectionTasks = new SqlConnection(connectionString))
            {
                string query = $"SELECT * FROM Task WHERE ProjectID = {projectID} AND MilestoneID = '{milestoneID}'";

                connectionTasks.Open();
                using (SqlCommand command = new SqlCommand(query, connectionTasks))
                {
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string taskName = reader["TaskName"].ToString();
                        milestoneNode.Nodes.Add(taskName);
                    }

                    reader.Close();
                }
            }
        }

        private void buttonProjeEkle_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageProjeEkle))
                tabControl.TabPages.Add(tabPageProjeEkle);
            tabControl.SelectedTab = tabPageProjeEkle;
            buttonProjeler.BackColor = Color.FromArgb(7, 123, 88);
            buttonKullanicilar.BackColor = Color.FromArgb(7, 123, 88);
        }

        private void comboBoxManager_Click(object sender, EventArgs e)
        {
            if(comboBoxManager.Items.Count == 0)
            {
                fillComboBoxPerson(comboBoxManager);
            }
        }

        private void fillComboBoxPerson(ComboBox comboBox)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT FirstName + ' ' + LastName AS ManagerName FROM Person";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBox.Items.Add(reader["ManagerName"].ToString());
                        }
                    }
                }
            }
        }

        private void comboBoxProjectTeam_Click(object sender, EventArgs e)
        {
            if (comboBoxProjectTeam.Items.Count == 0)
            {
                fillComboBoxPerson(comboBoxProjectTeam);
            }
        }

        private void buttonEkipUyeEkle_Click(object sender, EventArgs e)
        {
            string uye = comboBoxProjectTeam.Text;
            if(labelProjectTeam.Text == "")
            {
                labelProjectTeam.Text += uye;
            }
            else
            {
                labelProjectTeam.Text += $", {uye}";
            }
            comboBoxProjectTeam.Items.Remove(uye);
            comboBoxProjectTeam.Text = "";
        }

        private void buttonDokumanEkle_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Word Documents|*.docx|PDF Files|*.pdf|Excel Files|*.xlsx|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (labelDosya.Text == "")
                {
                    labelDosya.Text += openFileDialog.SafeFileName;
                }
                else
                {
                    labelDosya.Text += $", {openFileDialog.SafeFileName}";
                }
            }
        }

        private void buttonProjeyiEkle_Click(object sender, EventArgs e)
        {
            string projectName = textBoxProjectName.Text;
            string projectNumber = textBoxProjectNumber.Text;
            string projectManagerName = comboBoxManager.Text;
            string projectGoal = textBoxGoal.Text;
            string description = textBoxDescription.Text;
            string scope = textBoxScope.Text;
            DateTime registrationDate = dateTimePickerRegistiration.Value;
            DateTime projectStart = dateTimePickerStart.Value;
            DateTime projectEnd = dateTimePickerEnd.Value;
            DateTime estimatedStart = dateTimePickerEstStart.Value;
            DateTime estimatedEnd = dateTimePickerEstEnd.Value;
            int statusID = GetSelectedStatusID(GetSelectedRadioButton(panelProjectStatus)); 
            int monetaryReturn = Convert.ToInt32(textBoxMonetaryReturn.Text);
            int monetaryReturnTypeID = GetSelectedMonetaryReturnTypeID(GetSelectedRadioButton(panelReturnType));
            string projectDocuments = labelDosya.Text;
            int projectTypeID = GetSelectedProjectTypeID(GetSelectedRadioButton(panelProjectType));

            int projectID;
            dataAdapter = createDataAdapter(connection, null);
            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            DataRow lastRow = dataTable.Rows[dataTable.Rows.Count - 1];
            int lastProjectID = Convert.ToInt32(lastRow["ProjectID"]);
            projectID = lastProjectID + 1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string insertQuery = "INSERT INTO ProjectTable (ProjectID, ProjectName, ProjectNumber, ProjectManagerID, ProjectGoal, Description, Scope, RegistrationDate, ProjectStartDate, ProjectEndDate, EstimatedStart, EstimatedEnd, StatusID, MonetaryReturn, MonetaryReturnTypeID, Documents, ProjectTypeID) " +
                                     "VALUES (@ProjectID, @ProjectName, @ProjectNumber, @ProjectManagerID, @ProjectGoal, @Description, @Scope, @RegistrationDate, @ProjectStartDate, @ProjectEndDate, @EstimatedStart, @EstimatedEnd, @StatusID, @MonetaryReturn, @MonetaryReturnTypeID, @Documents, @ProjectTypeID)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectID);
                    command.Parameters.AddWithValue("@ProjectName", projectName);
                    command.Parameters.AddWithValue("@ProjectNumber", projectNumber);
                    command.Parameters.AddWithValue("@ProjectManagerID", GetProjectManagerID(projectManagerName)); // Bu fonksiyonunuzu projenizin gereksinimlerine göre uyarlayın
                    command.Parameters.AddWithValue("@ProjectGoal", projectGoal);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Scope", scope);
                    command.Parameters.AddWithValue("@RegistrationDate", registrationDate);
                    command.Parameters.AddWithValue("@ProjectStartDate", projectStart);
                    command.Parameters.AddWithValue("@ProjectEndDate", projectEnd);
                    command.Parameters.AddWithValue("@EstimatedStart", estimatedStart);
                    command.Parameters.AddWithValue("@EstimatedEnd", estimatedEnd);
                    command.Parameters.AddWithValue("@StatusID", statusID);
                    command.Parameters.AddWithValue("@MonetaryReturn", monetaryReturn);
                    command.Parameters.AddWithValue("@MonetaryReturnTypeID", monetaryReturnTypeID);
                    command.Parameters.AddWithValue("@Documents", projectDocuments);
                    command.Parameters.AddWithValue("@ProjectTypeID", projectTypeID);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Proje başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            connection = new SqlConnection(connectionString);
            dataAdapter = createDataAdapter(connection, null);

            dataTable = new DataTable();
            dataAdapter.Fill(dataTable);

            dataGridViewProjeler.DataSource = dataTable;
            dataGridViewProjeEkle.DataSource = dataTable;

            updateMilestoneTable(projectID);
            LoadProjects(connectionString);
        }

        private string GetSelectedRadioButton(Panel panel)
        {
            foreach (RadioButton radioButton in panel.Controls.OfType<RadioButton>())
            {
                if (radioButton.Checked)
                {
                    return radioButton.Text;
                }
            }

            return null;
        }


        private int GetSelectedStatusID(string statusName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID FROM Status WHERE Status = @StatusName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StatusName", statusName);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        private int GetSelectedMonetaryReturnTypeID(string returnType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID FROM MonetaryReturnType WHERE MonetaryReturnType = @ReturnType";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ReturnType", returnType);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        private int GetSelectedProjectTypeID(string projectType)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID FROM ProjectType WHERE ProjectType = @ProjectType";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectType", projectType);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        private int GetProjectManagerID(string projectManagerName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ID FROM Person WHERE FirstName + ' ' + LastName = @ProjectManagerName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectManagerName", projectManagerName);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        private int GetProjectID(string projectName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ProjectID FROM ProjectTable WHERE ProjectName = @ProjectName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProjectName", projectName);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
        }

        private void buttonMilestoneDuzenle_Click(object sender, EventArgs e)
        {
            if (textBoxProjectName.Text != "")
            {
                if (!tabControl.TabPages.Contains(tabPageMilestone))
                    tabControl.TabPages.Add(tabPageMilestone);
                tabControl.SelectedTab = tabPageMilestone;
                selectedProjectName = textBoxProjectName.Text;
                createMilestoneTreeView();
            }
            else
            {
                MessageBox.Show("Proje Adı boş olamaz.");
            }
        }



        private void buttonTaskEkle_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageTaskEkle))
                tabControl.TabPages.Add(tabPageTaskEkle);
            tabControl.SelectedTab = tabPageTaskEkle;
            textBoxTaskAdi.Text = "";
        }

        private void buttonListeyeEkle_Click(object sender, EventArgs e)
        {
            if(textBoxTaskAdi.Text != "")
            {
                tasks.Add(new Classes.Task(textBoxTaskAdi.Text, dateTimePickerTaskBaslangic.Value, dateTimePickerTaskBitis.Value));
                Classes.Task lastElement = tasks.ElementAt(tasks.Count - 1);
                listBoxTasklar.Items.Add(lastElement.toString());
            }
            else
            {
                MessageBox.Show("Task Adı boş olamaz.");
            }
        }

        private void buttonKilometreEkle_Click(object sender, EventArgs e)
        {
            Milestone milestone = new Milestone(textBoxKilometreTasi.Text, dateTimePickerKmBaslangic.Value, dateTimePickerKmBitis.Value, new List<Classes.Task>(tasks));
            milestones.Add(milestone);
            createMilestoneTreeView();
            textBoxKilometreTasi.Text = "";
            labelTasklar.Text = "";
            tasks.Clear();
            listBoxTasklar.Items.Clear();
        }

        private void createMilestoneTreeView()
        {
            string milestoneName;
            string taskName;
            treeViewKilometreTasi.Nodes.Clear();
            TreeNode projectNode = treeViewKilometreTasi.Nodes.Add(selectedProjectName);

            foreach (var milestoneItem in milestones){
                milestoneName = milestoneItem.MilestoneName;
                TreeNode milestoneNode = projectNode.Nodes.Add(milestoneName);

                foreach (var taskItem in milestoneItem.Tasks) {
                    taskName = taskItem.TaskName;
                    milestoneNode.Nodes.Add(taskName);
                }
            }
        }

        private void buttonTasklariEkle_Click(object sender, EventArgs e)
        {
            foreach (var itemTask in tasks)
            {
                if (labelTasklar.Text == "")
                {
                    labelTasklar.Text += itemTask.TaskName;
                }
                else
                {
                    labelTasklar.Text += $", {itemTask.TaskName}";
                }
            }
            tabControl.TabPages.Remove(tabPageTaskEkle);
            if (!tabControl.TabPages.Contains(tabPageMilestone))
                tabControl.TabPages.Add(tabPageMilestone);
            tabControl.SelectedTab = tabPageMilestone;
        }

        private void updateMilestoneTable(int ProjectID)
        {
            int lastMilestoneID = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT TOP 1 ID FROM Milestone ORDER BY ID DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        lastMilestoneID = Convert.ToInt32(result);
                    }
                }
            }

            int milestoneID = -1;
            int projectID = ProjectID;

            for (int i = 0; i < milestones.Count; i++)
            {
                milestoneID = lastMilestoneID + i + 1;
                string milestoneName = milestones[i].MilestoneName;
                DateTime milestoneStart = milestones[i].MilestoneStartDate;
                DateTime milestoneEnd = milestones[i].MilestoneEndDate;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO Milestone (ID, ProjectID, MilestoneName, StartDate, EndDate) " +
                                         "VALUES (@ID, @ProjectID, @MilestoneName, @StartDate, @EndDate)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID", milestoneID);
                        command.Parameters.AddWithValue("@ProjectID", projectID);
                        command.Parameters.AddWithValue("@MilestoneName", milestoneName);
                        command.Parameters.AddWithValue("@StartDate", milestoneStart);
                        command.Parameters.AddWithValue("@EndDate", milestoneEnd);

                        command.ExecuteNonQuery();
                    }
                }

                updateTaskTable(ProjectID, milestoneID, milestones[i]);
            }
        }

        private void updateTaskTable(int ProjectID, int MilestoneID, Milestone milestone)
        {
            int projectID = ProjectID;
            int milestoneID = MilestoneID;

            foreach (var task in milestone.Tasks)
            {
                string taskName = task.TaskName;
                DateTime taskStart = task.TaskStartDate;
                DateTime taskEnd = task.TaskEndDate;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO Task (TaskName, ProjectID, MilestoneID, StartDate, EndDate) " +
                                         "VALUES (@TaskName, @ProjectID, @MilestoneID, @StartDate, @EndDate)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TaskName", taskName);
                        command.Parameters.AddWithValue("@ProjectID", projectID);
                        command.Parameters.AddWithValue("@MilestoneID", milestoneID);
                        command.Parameters.AddWithValue("@StartDate", taskStart);
                        command.Parameters.AddWithValue("@EndDate", taskEnd);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        int selectedRowIndex = 0;
        private void buttonSil_Click(object sender, EventArgs e)
        {
            if (dataGridViewProjeler.SelectedRows.Count > 0)
            {
                string selectedProjectName = dataGridViewProjeler.Rows[selectedRowIndex].Cells["projectNameDataGridViewTextBoxColumn"].Value.ToString();

                DeleteProject(GetProjectID(selectedProjectName));

                MessageBox.Show("Proje başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                dataGridViewProjeler.DataSource = dataTable;
                dataGridViewProjeEkle.DataSource = dataTable;

                LoadProjects(connectionString);

            }
            else
            {
                MessageBox.Show("Lütfen bir satır seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteProject(int projectID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM ProjectTable WHERE ProjectID = @ProjectID";

                DeleteMilestone(projectID);

                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteMilestone(int projectID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Milestone WHERE ProjectID = @ProjectID";

                DeleteTask(projectID);

                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void DeleteTask(int projectID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Task WHERE ProjectID = @ProjectID";

                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                    command.ExecuteNonQuery();
                }
            }
        }

        private void dataGridViewProjeler_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewProjeler.Rows[e.RowIndex].Selected = true;
                selectedRowIndex = e.RowIndex;
            }
        }

        private void buttonProjeDuzenle_Click(object sender, EventArgs e)
        {
            if (dataGridViewProjeler.SelectedRows.Count > 0)
            {
                if (!tabControl.TabPages.Contains(tabPageProjeDuzenle))
                    tabControl.TabPages.Add(tabPageProjeDuzenle);
                tabControl.SelectedTab = tabPageProjeDuzenle;

                string selectedProjectName = dataGridViewProjeler.Rows[selectedRowIndex].Cells["projectNameDataGridViewTextBoxColumn"].Value.ToString();

                fillDuzenleElements(selectedProjectName);

                //DeleteProject(GetProjectID(selectedProjectName));

                //dataTable = new DataTable();
                //dataAdapter.Fill(dataTable);

                //dataGridViewProjeler.DataSource = dataTable;
                //dataGridViewProjeEkle.DataSource = dataTable;

                //LoadProjects(connectionString);

            }
            else
            {
                MessageBox.Show("Lütfen bir satır seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string GetProjectManagerName(int projectManagerID)
        {
            string projectManagerName = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT FirstName + ' ' + LastName AS ProjectManagerName FROM Person WHERE ID = @ProjectManagerID";

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectManagerID", projectManagerID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            projectManagerName = reader["ProjectManagerName"].ToString();
                        }
                    }
                }
            }

            return projectManagerName; // projectManagerName değerini döndür
        }


        private void fillDuzenleElements(string projectName)
        {
            int projectID = GetProjectID(projectName);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT * FROM ProjectTable WHERE ProjectID = @ProjectID";

                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@ProjectID", projectID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            textBoxDuzenleProjeAdi.Text = reader["ProjectName"].ToString();
                            textBoxDuzenleProjeNo.Text = reader["ProjectNumber"].ToString().Substring(3);
                            textBoxDuzenleTanim.Text = reader["Description"].ToString();
                            textBoxDuzenleHedef.Text = reader["ProjectGoal"].ToString();
                            textBoxDuzenleKapsam.Text = reader["Scope"].ToString();
                            dateTimePickerDuzenleKayit.Text = reader["RegistrationDate"].ToString();
                            dateTimePickerDuzenleStart.Text = reader["ProjectStartDate"].ToString();
                            dateTimePickerDuzenleEnd.Text = reader["ProjectEndDate"].ToString();
                            dateTimePickerDuzenleEstStart.Text = reader["EstimatedStart"].ToString();
                            dateTimePickerDuzenleEstEnd.Text = reader["EstimatedEnd"].ToString();
                            textBoxDuzenleGetiri.Text = reader["MonetaryReturn"].ToString();

                            int managerID = Convert.ToInt32(reader["ProjectManagerID"]);
                            comboBoxDuzenleManager.Text = GetProjectManagerName(managerID);

                            int statusID = Convert.ToInt32(reader["StatusID"]);
                            switch(statusID)
                            {
                                case 1:
                                    radioButtonDuzenleYapilacak.Checked = true;
                                    break;
                                case 2:
                                    radioButtonDuzenleDevamEdiyor.Checked = true;
                                    break;
                                case 3:
                                    radioButtonDuzenleOnay.Checked = true;
                                    break;
                                case 4:
                                    radioButtonDuzenleTamamlandi.Checked = true;
                                    break;
                                default:
                                    break;
                            }

                            int monetaryReturnTypeID = Convert.ToInt32(reader["MonetaryReturnTypeID"]);
                            switch (monetaryReturnTypeID)
                            {
                                case 1:
                                    radioButtonDuzenleGunluk.Checked = true;
                                    break;
                                case 2:
                                    radioButtonDuzenleAylik.Checked = true;
                                    break;
                                case 3:
                                    radioButtonDuzenleYillik.Checked = true;
                                    break;
                                default:
                                    break;
                            }

                            int projectTypeID = Convert.ToInt32(reader["ProjectTypeID"]);
                            switch (projectTypeID)
                            {
                                case 1:
                                    radioButtonDuzenleYurtdisi.Checked = true;
                                    break;
                                case 2:
                                    radioButtonDuzenleTubitak.Checked = true;
                                    break;
                                case 3:
                                    radioButtonDuzenleKobi.Checked = true;
                                    break;
                                default:
                                    break;
                            }

                            LoadData(projectID);
                        }   
                    }
                }
            }
        }

        private void LoadData(int projectID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (dataAdapter.SelectCommand != null && dataAdapter.SelectCommand.Connection != null && dataAdapter.SelectCommand.Connection.State == ConnectionState.Open)
                {
                    dataAdapter.SelectCommand.Connection.Close();
                }

                dataAdapter = createDataAdapter(connection, projectID);
                dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                dataGridViewDuzenlenmisProjeler.DataSource = dataTable;
            }
        }

        private void tabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < tabControl.TabPages.Count; i++)
                {
                    Rectangle r = tabControl.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        int tabIndex = i;

                        if (tabIndex != 0)
                        {
                            tabControl.TabPages.RemoveAt(tabIndex);
                        }

                        break;
                    }
                }
            }
        }

        private void buttonKisiEkle_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageKisiEkle))
                tabControl.TabPages.Add(tabPageKisiEkle);
            tabControl.SelectedTab = tabPageKisiEkle;
            buttonProjeler.BackColor = Color.FromArgb(7, 123, 88);
            buttonKullanicilar.BackColor = Color.FromArgb(7, 123, 88);
        }

        private void buttonKisiDuzenle_Click(object sender, EventArgs e)
        {
            if (!tabControl.TabPages.Contains(tabPageKisiDuzenle))
                tabControl.TabPages.Add(tabPageKisiDuzenle);
            tabControl.SelectedTab = tabPageKisiDuzenle;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            this.Hide();
            loginForm.Show();
        }
    }
}
