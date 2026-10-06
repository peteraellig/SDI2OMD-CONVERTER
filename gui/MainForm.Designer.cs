#nullable enable
namespace SdiOmt;
partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private Panel headerPanel = null!;
    private Label headerInfo = null!;
    private Label deviceLabel = null!;
    private ComboBox deviceBox = null!;
    private Button refreshButton = null!;
    private Label nameLabel = null!;
    private TextBox nameBox = null!;
    private Label modeLabel = null!;
    private ComboBox modeBox = null!;
    private Label qualityLabel = null!;
    private ComboBox qualityBox = null!;
    private FlowLayoutPanel optionsPanel = null!;
    private Label audioLabel = null!;
    private ComboBox audioBox = null!;
    private FlowLayoutPanel audioPanel = null!;
    private Label formatLabel = null!;
    private Button startButton = null!;
    private Button helpButton = null!;
    private Button exitButton = null!;
    private Button logButton = null!;
    private Button testButton = null!;
    private Label statusLabel = null!;
    private TextBox logBox = null!;
    private TableLayoutPanel layout = null!;
    private FlowLayoutPanel buttons = null!;
    private FlowLayoutPanel actionButtons = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        headerPanel = new Panel();
        headerInfo = new Label();
        deviceLabel = new Label();
        deviceBox = new ComboBox();
        refreshButton = new Button();
        nameLabel = new Label();
        nameBox = new TextBox();
        modeLabel = new Label(); modeBox = new ComboBox(); qualityLabel = new Label(); qualityBox = new ComboBox(); optionsPanel = new FlowLayoutPanel();
        audioLabel = new Label(); audioBox = new ComboBox(); audioPanel = new FlowLayoutPanel();
        formatLabel = new Label();
        startButton = new Button();
        helpButton = new Button();
        testButton = new Button(); logButton = new Button(); exitButton = new Button();
        statusLabel = new Label();
        logBox = new TextBox();
        layout = new TableLayoutPanel();
        buttons = new FlowLayoutPanel();
        actionButtons = new FlowLayoutPanel();
        pictureBox1 = new PictureBox();
        headerPanel.SuspendLayout();
        layout.SuspendLayout();
        buttons.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(33, 37, 41);
        headerPanel.Controls.Add(pictureBox1);
        headerPanel.Controls.Add(headerInfo);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Size = new Size(800, 64);
        headerPanel.TabIndex = 1;
        // 
        // headerInfo
        // 
        headerInfo.Dock = DockStyle.Right;
        headerInfo.Font = new Font("Segoe UI", 10F);
        headerInfo.ForeColor = Color.FromArgb(190, 195, 200);
        headerInfo.Location = new Point(610, 0);
        headerInfo.Name = "headerInfo";
        headerInfo.Size = new Size(190, 64);
        headerInfo.TabIndex = 1;
        headerInfo.Text = "720p50 · STEREO";
        headerInfo.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // deviceLabel
        // 
        deviceLabel.Anchor = AnchorStyles.Left;
        deviceLabel.AutoSize = true;
        deviceLabel.Location = new Point(23, 29);
        deviceLabel.Name = "deviceLabel";
        deviceLabel.Size = new Size(103, 15);
        deviceLabel.TabIndex = 0;
        deviceLabel.Text = "DeckLink input";
        // 
        // deviceBox
        // 
        deviceBox.DisplayMember = "Name";
        deviceBox.Dock = DockStyle.Fill;
        deviceBox.DropDownStyle = ComboBoxStyle.DropDownList;
        deviceBox.Location = new Point(168, 23);
        deviceBox.Name = "deviceBox";
        deviceBox.Size = new Size(516, 23);
        deviceBox.TabIndex = 1;
        // 
        // refreshButton
        // 
        refreshButton.AutoSize = true;
        refreshButton.BackColor = Color.White;
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.Location = new Point(690, 23);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(87, 27);
        refreshButton.TabIndex = 2;
        refreshButton.Text = "Refresh";
        refreshButton.UseVisualStyleBackColor = false;
        refreshButton.Click += RefreshButton_Click;
        // 
        // nameLabel
        // 
        nameLabel.Anchor = AnchorStyles.Left;
        nameLabel.AutoSize = true;
        nameLabel.Location = new Point(23, 60);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(105, 15);
        nameLabel.TabIndex = 3;
        nameLabel.Text = "OMT sender name";
        // 
        // nameBox
        // 
        layout.SetColumnSpan(nameBox, 3);
        nameBox.Dock = DockStyle.Fill;
        nameBox.Location = new Point(168, 56);
        nameBox.MaxLength = 120;
        nameBox.Name = "nameBox";
        nameBox.Size = new Size(609, 23);
        nameBox.TabIndex = 4;
        nameBox.Text = "OMT-SDI";
        // 
        // formatLabel
        // 
        formatLabel.AutoSize = true;
        layout.SetColumnSpan(formatLabel, 3);
        formatLabel.Dock = DockStyle.Fill;
        formatLabel.Location = new Point(20, 94);
        formatLabel.Margin = new Padding(0, 12, 0, 12);
        formatLabel.Name = "formatLabel";
        formatLabel.Size = new Size(760, 30);
        formatLabel.TabIndex = 5;
        formatLabel.Text = "Stereo 48 kHz · Manual format selection · No auto-detection. The input must be available in the selected format.";
        formatLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        // 
        // startButton
        // 
        startButton.AutoSize = true;
        startButton.BackColor = Color.FromArgb(35, 125, 75);
        startButton.FlatAppearance.BorderSize = 0;
        startButton.FlatStyle = FlatStyle.Flat;
        startButton.ForeColor = Color.White;
        startButton.Location = new Point(3, 3);
        startButton.Name = "startButton";
        startButton.Padding = new Padding(12, 8, 12, 8);
        startButton.Size = new Size(147, 41);
        startButton.TabIndex = 0;
        startButton.Text = "Start transmission";
        startButton.UseVisualStyleBackColor = false;
        startButton.Click += StartButton_Click;
        // 
        // helpButton
        //
        helpButton.AutoSize = true;
        helpButton.BackColor = Color.White;
        helpButton.ForeColor = Color.FromArgb(33, 37, 41);
        helpButton.FlatStyle = FlatStyle.Flat;
        helpButton.Padding = new Padding(12, 8, 12, 8);
        helpButton.Name = "helpButton";
        helpButton.Text = "Help";
        helpButton.TabIndex = 3;
        helpButton.UseVisualStyleBackColor = false;
        helpButton.Click += HelpButton_Click;
        //
        // testButton
        // 
        testButton.AutoSize = true;
        testButton.BackColor = Color.White;
        testButton.FlatAppearance.BorderSize = 0;
        testButton.FlatStyle = FlatStyle.Flat;
        testButton.ForeColor = Color.FromArgb(33, 37, 41);
        testButton.Location = new Point(247, 3);
        testButton.Name = "testButton";
        testButton.Padding = new Padding(12, 8, 12, 8);
        testButton.Size = new Size(123, 41);
        testButton.TabIndex = 2;
        testButton.Text = "OMT self-test";
        testButton.UseVisualStyleBackColor = false;
        testButton.Click += TestButton_Click;
        // 
        // statusLabel
        // 
        statusLabel.AutoSize = true;
        layout.SetColumnSpan(statusLabel, 4);
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        statusLabel.Location = new Point(20, 201);
        statusLabel.Margin = new Padding(0, 12, 0, 12);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(760, 19);
        statusLabel.TabIndex = 7;
        statusLabel.Text = "Ready";
        // 
        // logBox
        // 
        logBox.BackColor = SystemColors.Window;
        layout.SetColumnSpan(logBox, 4);
        logBox.Dock = DockStyle.Fill;
        logBox.Font = new Font("Consolas", 10F);
        logBox.Location = new Point(23, 235);
        logBox.Multiline = true;
        logBox.Name = "logBox";
        logBox.ReadOnly = true; logBox.Visible = false;
        logBox.ScrollBars = ScrollBars.Vertical;
        logBox.Size = new Size(754, 218);
        logBox.TabIndex = 8;
        // 
        // layout
        // 
        layout.ColumnCount = 4;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        layout.Controls.Add(deviceLabel, 0, 1);
        layout.Controls.Add(deviceBox, 1, 1); layout.SetColumnSpan(deviceBox, 2);
        layout.Controls.Add(refreshButton, 3, 1);
        refreshButton.Anchor = AnchorStyles.Right;
        layout.Controls.Add(nameLabel, 0, 2);
        layout.Controls.Add(nameBox, 1, 2);
        modeLabel.Text = "Video format"; modeLabel.AutoSize = true; modeLabel.Margin = new Padding(0, 7, 12, 0);
        modeBox.Name = "modeBox"; modeBox.DropDownStyle = ComboBoxStyle.DropDownList; modeBox.Width = 130;
        modeBox.Items.AddRange(new object[] { "720p50", "720p60", "1080p50", "1080p60", "1080i50", "1080i60" });
        qualityLabel.Text = "OMT quality"; qualityLabel.AutoSize = true; qualityLabel.Margin = new Padding(24, 7, 12, 0);
        qualityBox.Name = "qualityBox"; qualityBox.DropDownStyle = ComboBoxStyle.DropDownList; qualityBox.Width = 110;
        qualityBox.Items.AddRange(new object[] { "High", "Normal", "Low" });
        optionsPanel.Name = "optionsPanel"; optionsPanel.Dock = DockStyle.Fill; optionsPanel.AutoSize = true;
        optionsPanel.Controls.Add(modeLabel); optionsPanel.Controls.Add(modeBox); optionsPanel.Controls.Add(qualityLabel); optionsPanel.Controls.Add(qualityBox);
        layout.Controls.Add(optionsPanel, 0, 3); layout.SetColumnSpan(optionsPanel, 3);
        audioLabel.Text = "Audio channels"; audioLabel.AutoSize = true; audioLabel.Margin = new Padding(0, 7, 12, 0);
        audioBox.Name = "audioBox"; audioBox.DropDownStyle = ComboBoxStyle.DropDownList; audioBox.Width = 180;
        audioBox.Items.AddRange(new object[] { "Stereo 1&2", "Stereo 3&4", "Stereo 5&6", "Stereo 7&8", "Mono 1", "Mono 2", "Mono 3", "Mono 4", "Mono 5", "Mono 6", "Mono 7", "Mono 8" });
        audioPanel.Name = "audioPanel"; audioPanel.Dock = DockStyle.Fill; audioPanel.AutoSize = true;
        audioPanel.Controls.Add(audioLabel); audioPanel.Controls.Add(audioBox);
        layout.Controls.Add(audioPanel, 0, 4); layout.SetColumnSpan(audioPanel, 3);
        layout.Controls.Add(formatLabel, 0, 5);
        layout.Controls.Add(buttons, 0, 6);
        layout.Controls.Add(statusLabel, 0, 7);
        layout.Controls.Add(logBox, 0, 8);
        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 64);
        layout.Name = "layout";
        layout.Padding = new Padding(20);
        layout.RowCount = 9;
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle());
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Size = new Size(800, 476);
        layout.TabIndex = 0;
        // 
        // buttons
        // 
        buttons.AutoSize = true;
        layout.SetColumnSpan(buttons, 3);
        buttons.Controls.Add(startButton);
        
        
        logButton.Name = "logButton"; logButton.Text = "Show list"; logButton.AutoSize = true;
        logButton.BackColor = Color.White; logButton.ForeColor = Color.FromArgb(33, 37, 41);
        logButton.FlatStyle = FlatStyle.Flat; logButton.Padding = new Padding(12, 8, 12, 8);
        logButton.Click += LogButton_Click; 
        exitButton.Name = "exitButton"; exitButton.Text = "Exit"; exitButton.AutoSize = true;
        exitButton.BackColor = Color.IndianRed; exitButton.ForeColor = Color.White;
        exitButton.FlatStyle = FlatStyle.Flat; exitButton.FlatAppearance.BorderSize = 0;
        exitButton.Padding = new Padding(12, 8, 12, 8); exitButton.UseVisualStyleBackColor = false;
        exitButton.Click += ExitButton_Click; 
        buttons.Dock = DockStyle.Fill;
        buttons.Location = new Point(23, 139);
        buttons.Name = "buttons";
        buttons.Size = new Size(754, 47);
        buttons.TabIndex = 6;
        actionButtons.Name = "actionButtons";
        actionButtons.FlowDirection = FlowDirection.TopDown;
        actionButtons.WrapContents = false;
        actionButtons.AutoSize = true;
        actionButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        actionButtons.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        actionButtons.Margin = new Padding(12, 3, 3, 3);
        actionButtons.Controls.Add(testButton);
        actionButtons.Controls.Add(logButton);
        actionButtons.Controls.Add(helpButton);
        actionButtons.Controls.Add(exitButton);
        testButton.AutoSize = false; testButton.Size = new Size(125, 41); testButton.Margin = new Padding(0, 0, 0, 6);
        logButton.AutoSize = false; logButton.Size = new Size(125, 41); logButton.Margin = new Padding(0, 0, 0, 6);
        helpButton.AutoSize = false; helpButton.Size = new Size(125, 41); helpButton.Margin = new Padding(0, 0, 0, 6);
        exitButton.AutoSize = false; exitButton.Size = new Size(125, 41); exitButton.Margin = new Padding(0);
        layout.Controls.Add(actionButtons, 3, 3);
        layout.SetRowSpan(actionButtons, 4);
        // 
        // pictureBox1
        // 
        pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
        pictureBox1.Image = Properties.Resources.sdi_omt_logo_ws;
        pictureBox1.Location = new Point(12, 2);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(140, 56);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 2;
        pictureBox1.TabStop = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(240, 242, 245);
        ClientSize = new Size(800, 540);
        Controls.Add(layout);
        Controls.Add(headerPanel);
        Font = new Font("Segoe UI", 9F);
        Icon = (Icon?)resources.GetObject("$this.Icon");
        MinimumSize = new Size(700, 460);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SDI2OMD CONVERTER";
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        headerPanel.ResumeLayout(false);
        layout.ResumeLayout(false);
        layout.PerformLayout();
        buttons.ResumeLayout(false);
        buttons.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
    }

    private PictureBox pictureBox1 = null!;
}



