#nullable enable
namespace SdiOmt;
partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private Panel headerPanel = null!;
    private PictureBox pictureBox1 = null!;
    private Label headerInfo = null!;
    private Label statusLabel = null!;
    private TableLayoutPanel layout = null!;
    private Label deviceLabel = null!;
    private Label nameLabel = null!;
    private Label modeLabel = null!;
    private Label qualityLabel = null!;
    private Label audioLabel = null!;
    private ComboBox deviceBox = null!;
    private TextBox nameBox = null!;
    private ComboBox modeBox = null!;
    private ComboBox qualityBox = null!;
    private ComboBox audioBox = null!;
    private Button refreshButton = null!;
    private Button testButton = null!;
    private Button cardInfoButton = null!;
    private Button logButton = null!;
    private Button helpButton = null!;
    private Button exitButton = null!;
    private Button startButton = null!;
    private Label formatLabel = null!;
    private StatusStrip systemStrip = null!;
    private StatusStrip streamStrip = null!;
    private ToolStripStatusLabel cpuCell = null!;
    private ToolStripStatusLabel ramCell = null!;
    private ToolStripStatusLabel freeRamCell = null!;
    private ToolStripStatusLabel encoderCpuCell = null!;
    private ToolStripStatusLabel encoderRamCell = null!;
    private ToolStripStatusLabel connectionsCell = null!;
    private ToolStripStatusLabel videoLossCell = null!;
    private ToolStripStatusLabel audioLossCell = null!;
    private ToolStripStatusLabel errorsCell = null!;
    private ToolStripStatusLabel videoQueueCell = null!;
    private ToolStripStatusLabel audioQueueCell = null!;
    private ToolStripStatusLabel sendCell = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            healthTimer.Stop();
            healthTimer.Dispose();
            logBox?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        headerPanel = new Panel();
        statusLabel = new Label();
        headerInfo = new Label();
        pictureBox1 = new PictureBox();
        layout = new TableLayoutPanel();
        deviceLabel = new Label();
        deviceBox = new ComboBox();
        refreshButton = new Button();
        nameLabel = new Label();
        nameBox = new TextBox();
        testButton = new Button();
        modeLabel = new Label();
        modeBox = new ComboBox();
        cardInfoButton = new Button();
        qualityLabel = new Label();
        qualityBox = new ComboBox();
        logButton = new Button();
        audioLabel = new Label();
        audioBox = new ComboBox();
        helpButton = new Button();
        exitButton = new Button();
        startButton = new Button();
        formatLabel = new Label();
        systemStrip = new StatusStrip();
        cpuCell = new ToolStripStatusLabel();
        ramCell = new ToolStripStatusLabel();
        freeRamCell = new ToolStripStatusLabel();
        encoderCpuCell = new ToolStripStatusLabel();
        encoderRamCell = new ToolStripStatusLabel();
        connectionsCell = new ToolStripStatusLabel();
        streamStrip = new StatusStrip();
        videoLossCell = new ToolStripStatusLabel();
        audioLossCell = new ToolStripStatusLabel();
        errorsCell = new ToolStripStatusLabel();
        videoQueueCell = new ToolStripStatusLabel();
        audioQueueCell = new ToolStripStatusLabel();
        sendCell = new ToolStripStatusLabel();
        headerPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        layout.SuspendLayout();
        systemStrip.SuspendLayout();
        streamStrip.SuspendLayout();
        SuspendLayout();
        //
        // headerPanel
        //
        headerPanel.BackColor = Color.FromArgb(33, 37, 41);
        headerPanel.Controls.Add(statusLabel);
        headerPanel.Controls.Add(headerInfo);
        headerPanel.Controls.Add(pictureBox1);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Size = new Size(820, 64);
        headerPanel.TabIndex = 1;
        //
        // statusLabel
        //
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        statusLabel.ForeColor = Color.Gainsboro;
        statusLabel.Location = new Point(180, 0);
        statusLabel.Name = "statusLabel";
        statusLabel.Padding = new Padding(6, 0, 6, 0);
        statusLabel.Size = new Size(460, 64);
        statusLabel.TabIndex = 0;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleCenter;
        //
        // headerInfo
        //
        headerInfo.Dock = DockStyle.Right;
        headerInfo.ForeColor = Color.Silver;
        headerInfo.Location = new Point(640, 0);
        headerInfo.Name = "headerInfo";
        headerInfo.Size = new Size(180, 64);
        headerInfo.TabIndex = 1;
        headerInfo.TextAlign = ContentAlignment.MiddleCenter;
        //
        // pictureBox1
        //
        pictureBox1.Dock = DockStyle.Left;
        pictureBox1.Image = Properties.Resources.sdi_omt_logo_ws;
        pictureBox1.Location = new Point(0, 0);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(180, 64);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 2;
        pictureBox1.TabStop = false;
        //
        // layout
        //
        layout.ColumnCount = 3;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
        layout.Controls.Add(deviceLabel, 0, 0);
        layout.Controls.Add(deviceBox, 1, 0);
        layout.Controls.Add(refreshButton, 2, 0);
        layout.Controls.Add(nameLabel, 0, 1);
        layout.Controls.Add(nameBox, 1, 1);
        layout.Controls.Add(testButton, 2, 1);
        layout.Controls.Add(modeLabel, 0, 2);
        layout.Controls.Add(modeBox, 1, 2);
        layout.Controls.Add(cardInfoButton, 2, 2);
        layout.Controls.Add(qualityLabel, 0, 3);
        layout.Controls.Add(qualityBox, 1, 3);
        layout.Controls.Add(logButton, 2, 3);
        layout.Controls.Add(audioLabel, 0, 4);
        layout.Controls.Add(audioBox, 1, 4);
        layout.Controls.Add(helpButton, 2, 4);
        layout.Controls.Add(exitButton, 2, 5);
        layout.Controls.Add(startButton, 0, 5);
        layout.Controls.Add(formatLabel, 0, 6);
        layout.Dock = DockStyle.Fill;
        layout.Location = new Point(0, 64);
        layout.Name = "layout";
        layout.Padding = new Padding(20, 14, 20, 10);
        layout.RowCount = 7;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Size = new Size(820, 335);
        layout.TabIndex = 0;
        //
        // deviceLabel
        //
        deviceLabel.Dock = DockStyle.Fill;
        deviceLabel.Location = new Point(20, 14);
        deviceLabel.Margin = new Padding(0);
        deviceLabel.Name = "deviceLabel";
        deviceLabel.Size = new Size(145, 43);
        deviceLabel.TabIndex = 0;
        deviceLabel.Text = "DeckLink input";
        deviceLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // deviceBox
        //
        deviceBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        deviceBox.DisplayMember = "Name";
        deviceBox.DropDownStyle = ComboBoxStyle.DropDownList;
        deviceBox.Location = new Point(165, 24);
        deviceBox.Margin = new Padding(0, 0, 20, 0);
        deviceBox.Name = "deviceBox";
        deviceBox.Size = new Size(455, 23);
        deviceBox.TabIndex = 1;
        //
        // refreshButton
        //
        refreshButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        refreshButton.BackColor = Color.White;
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.ForeColor = Color.FromArgb(33, 37, 41);
        refreshButton.Location = new Point(640, 18);
        refreshButton.Margin = new Padding(0);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(160, 35);
        refreshButton.TabIndex = 2;
        refreshButton.Text = "Refresh";
        refreshButton.UseVisualStyleBackColor = false;
        refreshButton.Click += RefreshButton_Click;
        //
        // nameLabel
        //
        nameLabel.Dock = DockStyle.Fill;
        nameLabel.Location = new Point(20, 57);
        nameLabel.Margin = new Padding(0);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(145, 43);
        nameLabel.TabIndex = 3;
        nameLabel.Text = "OMT sender name";
        nameLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // nameBox
        //
        nameBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        nameBox.Location = new Point(165, 67);
        nameBox.Margin = new Padding(0, 0, 20, 0);
        nameBox.MaxLength = 120;
        nameBox.Name = "nameBox";
        nameBox.Size = new Size(455, 23);
        nameBox.TabIndex = 4;
        nameBox.Text = "OMT-SDI";
        nameBox.TextAlign = HorizontalAlignment.Left;
        //
        // testButton
        //
        testButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        testButton.BackColor = Color.White;
        testButton.FlatStyle = FlatStyle.Flat;
        testButton.ForeColor = Color.FromArgb(33, 37, 41);
        testButton.Location = new Point(640, 61);
        testButton.Margin = new Padding(0);
        testButton.Name = "testButton";
        testButton.Size = new Size(160, 35);
        testButton.TabIndex = 5;
        testButton.Text = "OMT self-test";
        testButton.UseVisualStyleBackColor = false;
        testButton.Click += TestButton_Click;
        //
        // modeLabel
        //
        modeLabel.Dock = DockStyle.Fill;
        modeLabel.Location = new Point(20, 100);
        modeLabel.Margin = new Padding(0);
        modeLabel.Name = "modeLabel";
        modeLabel.Size = new Size(145, 43);
        modeLabel.TabIndex = 6;
        modeLabel.Text = "Video format";
        modeLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // modeBox
        //
        modeBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        modeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        modeBox.Items.AddRange(new object[] { "720p50", "720p60", "1080p50", "1080p60", "1080i50", "1080i60" });
        modeBox.Location = new Point(165, 110);
        modeBox.Margin = new Padding(0, 0, 20, 0);
        modeBox.Name = "modeBox";
        modeBox.Size = new Size(455, 23);
        modeBox.TabIndex = 7;
        //
        // cardInfoButton
        //
        cardInfoButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cardInfoButton.BackColor = Color.White;
        cardInfoButton.FlatStyle = FlatStyle.Flat;
        cardInfoButton.ForeColor = Color.FromArgb(33, 37, 41);
        cardInfoButton.Location = new Point(640, 104);
        cardInfoButton.Margin = new Padding(0);
        cardInfoButton.Name = "cardInfoButton";
        cardInfoButton.Size = new Size(160, 35);
        cardInfoButton.TabIndex = 8;
        cardInfoButton.Text = "BM card info";
        cardInfoButton.UseVisualStyleBackColor = false;
        cardInfoButton.Click += CardInfoButton_Click;
        //
        // qualityLabel
        //
        qualityLabel.Dock = DockStyle.Fill;
        qualityLabel.Location = new Point(20, 143);
        qualityLabel.Margin = new Padding(0);
        qualityLabel.Name = "qualityLabel";
        qualityLabel.Size = new Size(145, 43);
        qualityLabel.TabIndex = 9;
        qualityLabel.Text = "OMT quality";
        qualityLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // qualityBox
        //
        qualityBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        qualityBox.DropDownStyle = ComboBoxStyle.DropDownList;
        qualityBox.Items.AddRange(new object[] { "High", "Normal", "Low" });
        qualityBox.Location = new Point(165, 153);
        qualityBox.Margin = new Padding(0, 0, 20, 0);
        qualityBox.Name = "qualityBox";
        qualityBox.Size = new Size(455, 23);
        qualityBox.TabIndex = 10;
        //
        // logButton
        //
        logButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        logButton.BackColor = Color.White;
        logButton.FlatStyle = FlatStyle.Flat;
        logButton.ForeColor = Color.FromArgb(33, 37, 41);
        logButton.Location = new Point(640, 147);
        logButton.Margin = new Padding(0);
        logButton.Name = "logButton";
        logButton.Size = new Size(160, 35);
        logButton.TabIndex = 11;
        logButton.Text = "Show list";
        logButton.UseVisualStyleBackColor = false;
        logButton.Click += LogButton_Click;
        //
        // audioLabel
        //
        audioLabel.Dock = DockStyle.Fill;
        audioLabel.Location = new Point(20, 186);
        audioLabel.Margin = new Padding(0);
        audioLabel.Name = "audioLabel";
        audioLabel.Size = new Size(145, 43);
        audioLabel.TabIndex = 12;
        audioLabel.Text = "Audio channels";
        audioLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // audioBox
        //
        audioBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        audioBox.DropDownStyle = ComboBoxStyle.DropDownList;
        audioBox.Items.AddRange(new object[] { "Stereo 1&2", "Stereo 3&4", "Stereo 5&6", "Stereo 7&8", "Mono 1", "Mono 2", "Mono 3", "Mono 4", "Mono 5", "Mono 6", "Mono 7", "Mono 8" });
        audioBox.Location = new Point(165, 196);
        audioBox.Margin = new Padding(0, 0, 20, 0);
        audioBox.Name = "audioBox";
        audioBox.Size = new Size(455, 23);
        audioBox.TabIndex = 13;
        //
        // helpButton
        //
        helpButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        helpButton.BackColor = Color.White;
        helpButton.FlatStyle = FlatStyle.Flat;
        helpButton.ForeColor = Color.FromArgb(33, 37, 41);
        helpButton.Location = new Point(640, 190);
        helpButton.Margin = new Padding(0);
        helpButton.Name = "helpButton";
        helpButton.Size = new Size(160, 35);
        helpButton.TabIndex = 14;
        helpButton.Text = "Help";
        helpButton.UseVisualStyleBackColor = false;
        helpButton.Click += HelpButton_Click;
        //
        // exitButton
        //
        exitButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        exitButton.BackColor = Color.IndianRed;
        exitButton.FlatAppearance.BorderSize = 0;
        exitButton.FlatStyle = FlatStyle.Flat;
        exitButton.ForeColor = Color.White;
        exitButton.Location = new Point(640, 233);
        exitButton.Margin = new Padding(0);
        exitButton.Name = "exitButton";
        exitButton.Size = new Size(160, 35);
        exitButton.TabIndex = 15;
        exitButton.Text = "Exit";
        exitButton.UseVisualStyleBackColor = false;
        exitButton.Click += ExitButton_Click;
        //
        // startButton
        //
        startButton.Anchor = AnchorStyles.Left;
        startButton.BackColor = Color.FromArgb(35, 125, 75);
        layout.SetColumnSpan(startButton, 2);
        startButton.FlatAppearance.BorderSize = 0;
        startButton.FlatStyle = FlatStyle.Flat;
        startButton.ForeColor = Color.White;
        startButton.Location = new Point(20, 233);
        startButton.Margin = new Padding(0);
        startButton.Name = "startButton";
        startButton.Size = new Size(205, 35);
        startButton.TabIndex = 16;
        startButton.Text = "Start transmission";
        startButton.UseVisualStyleBackColor = false;
        startButton.Click += StartButton_Click;
        //
        // formatLabel
        //
        layout.SetColumnSpan(formatLabel, 3);
        formatLabel.Dock = DockStyle.Fill;
        formatLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        formatLabel.Location = new Point(20, 272);
        formatLabel.Margin = new Padding(0);
        formatLabel.Name = "formatLabel";
        formatLabel.Size = new Size(780, 53);
        formatLabel.TabIndex = 17;
        formatLabel.Text = "Stereo 48 kHz · Manual format selection · No auto-detection. The input must be available in the selected format.";
        formatLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // systemStrip
        //
        systemStrip.AutoSize = false;
        systemStrip.Items.AddRange(new ToolStripItem[] { cpuCell, ramCell, freeRamCell, encoderCpuCell, encoderRamCell, connectionsCell });
        systemStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        systemStrip.Location = new Point(0, 399);
        systemStrip.Name = "systemStrip";
        systemStrip.Padding = new Padding(0);
        systemStrip.Size = new Size(820, 36);
        systemStrip.SizingGrip = false;
        systemStrip.TabIndex = 2;
        systemStrip.SizeChanged += StatusStrip_SizeChanged;
        //
        // cpuCell
        //
        cpuCell.AutoSize = false;
        cpuCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        cpuCell.Margin = new Padding(0);
        cpuCell.Name = "cpuCell";
        cpuCell.Overflow = ToolStripItemOverflow.Never;
        cpuCell.Size = new Size(136, 35);
        cpuCell.Tag = "CPU · system";
        cpuCell.Text = "CPU · system\n—";
        //
        // ramCell
        //
        ramCell.AutoSize = false;
        ramCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        ramCell.Margin = new Padding(0);
        ramCell.Name = "ramCell";
        ramCell.Overflow = ToolStripItemOverflow.Never;
        ramCell.Size = new Size(136, 35);
        ramCell.Tag = "RAM · system";
        ramCell.Text = "RAM · system\n—";
        //
        // freeRamCell
        //
        freeRamCell.AutoSize = false;
        freeRamCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        freeRamCell.Margin = new Padding(0);
        freeRamCell.Name = "freeRamCell";
        freeRamCell.Overflow = ToolStripItemOverflow.Never;
        freeRamCell.Size = new Size(136, 35);
        freeRamCell.Tag = "RAM · free";
        freeRamCell.Text = "RAM · free\n—";
        //
        // encoderCpuCell
        //
        encoderCpuCell.AutoSize = false;
        encoderCpuCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        encoderCpuCell.Margin = new Padding(0);
        encoderCpuCell.Name = "encoderCpuCell";
        encoderCpuCell.Overflow = ToolStripItemOverflow.Never;
        encoderCpuCell.Size = new Size(136, 35);
        encoderCpuCell.Tag = "CPU · encoder";
        encoderCpuCell.Text = "CPU · encoder\n—";
        //
        // encoderRamCell
        //
        encoderRamCell.AutoSize = false;
        encoderRamCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        encoderRamCell.Margin = new Padding(0);
        encoderRamCell.Name = "encoderRamCell";
        encoderRamCell.Overflow = ToolStripItemOverflow.Never;
        encoderRamCell.Size = new Size(136, 35);
        encoderRamCell.Tag = "RAM · encoder";
        encoderRamCell.Text = "RAM · encoder\n—";
        //
        // connectionsCell
        //
        connectionsCell.AutoSize = false;
        connectionsCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        connectionsCell.Margin = new Padding(0);
        connectionsCell.Name = "connectionsCell";
        connectionsCell.Overflow = ToolStripItemOverflow.Never;
        connectionsCell.Size = new Size(136, 35);
        connectionsCell.Tag = "OMT channels";
        connectionsCell.Text = "OMT channels\n—";
        //
        // streamStrip
        //
        streamStrip.AutoSize = false;
        streamStrip.Items.AddRange(new ToolStripItem[] { videoLossCell, audioLossCell, errorsCell, videoQueueCell, audioQueueCell, sendCell });
        streamStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
        streamStrip.Location = new Point(0, 435);
        streamStrip.Name = "streamStrip";
        streamStrip.Padding = new Padding(0);
        streamStrip.Size = new Size(820, 35);
        streamStrip.SizingGrip = false;
        streamStrip.TabIndex = 3;
        streamStrip.SizeChanged += StatusStrip_SizeChanged;
        //
        // videoLossCell
        //
        videoLossCell.AutoSize = false;
        videoLossCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        videoLossCell.Margin = new Padding(0);
        videoLossCell.Name = "videoLossCell";
        videoLossCell.Overflow = ToolStripItemOverflow.Never;
        videoLossCell.Size = new Size(136, 35);
        videoLossCell.Tag = "Lost frames · 5 s";
        videoLossCell.Text = "Lost frames · 5 s\n—";
        //
        // audioLossCell
        //
        audioLossCell.AutoSize = false;
        audioLossCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        audioLossCell.Margin = new Padding(0);
        audioLossCell.Name = "audioLossCell";
        audioLossCell.Overflow = ToolStripItemOverflow.Never;
        audioLossCell.Size = new Size(136, 35);
        audioLossCell.Tag = "Audio gaps · 5 s";
        audioLossCell.Text = "Audio gaps · 5 s\n—";
        //
        // errorsCell
        //
        errorsCell.AutoSize = false;
        errorsCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        errorsCell.Margin = new Padding(0);
        errorsCell.Name = "errorsCell";
        errorsCell.Overflow = ToolStripItemOverflow.Never;
        errorsCell.Size = new Size(136, 35);
        errorsCell.Tag = "Errors · 5 s";
        errorsCell.Text = "Errors · 5 s\n—";
        //
        // videoQueueCell
        //
        videoQueueCell.AutoSize = false;
        videoQueueCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        videoQueueCell.Margin = new Padding(0);
        videoQueueCell.Name = "videoQueueCell";
        videoQueueCell.Overflow = ToolStripItemOverflow.Never;
        videoQueueCell.Size = new Size(136, 35);
        videoQueueCell.Tag = "Video waiting";
        videoQueueCell.Text = "Video waiting\n—";
        //
        // audioQueueCell
        //
        audioQueueCell.AutoSize = false;
        audioQueueCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        audioQueueCell.Margin = new Padding(0);
        audioQueueCell.Name = "audioQueueCell";
        audioQueueCell.Overflow = ToolStripItemOverflow.Never;
        audioQueueCell.Size = new Size(136, 35);
        audioQueueCell.Tag = "Audio waiting";
        audioQueueCell.Text = "Audio waiting\n—";
        //
        // sendCell
        //
        sendCell.AutoSize = false;
        sendCell.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
        sendCell.Margin = new Padding(0);
        sendCell.Name = "sendCell";
        sendCell.Overflow = ToolStripItemOverflow.Never;
        sendCell.Size = new Size(136, 35);
        sendCell.Tag = "Last send";
        sendCell.Text = "Last send\n—";
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(240, 242, 245);
        ClientSize = new Size(820, 470);
        Controls.Add(layout);
        Controls.Add(headerPanel);
        Controls.Add(systemStrip);
        Controls.Add(streamStrip);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = (Icon?)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SDI2OMD CONVERTER";
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        headerPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        layout.ResumeLayout(false);
        layout.PerformLayout();
        systemStrip.ResumeLayout(false);
        systemStrip.PerformLayout();
        streamStrip.ResumeLayout(false);
        streamStrip.PerformLayout();
        ResumeLayout(false);
    }
}
