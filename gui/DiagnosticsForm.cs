namespace SdiOmt;
public sealed class DiagnosticsForm : Form
{
    private readonly TextBox log;
    public DiagnosticsForm(TextBox logBox)
    {
        log=logBox;
        Text="SDI2OMD CONVERTER — Diagnostics";Font=new Font("Segoe UI",9F);
        BackColor=Color.FromArgb(240,242,245);ClientSize=new Size(850,400);MinimumSize=new Size(650,280);
        StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;MaximizeBox=false;MinimizeBox=false;
        var layout=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=new Padding(12),ColumnCount=1,RowCount=3};
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        layout.RowStyles.Add(new RowStyle());layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle());
        layout.Controls.Add(new Label {AutoSize=true,Text="Live diagnostics · Close turns detailed logging off",Margin=new Padding(0,0,0,8)},0,0);
        log.Visible=true;log.Dock=DockStyle.Fill;layout.Controls.Add(log,0,1);
        var close=new Button {Name="closeButton",Text="Close",Size=new Size(100,32),Anchor=AnchorStyles.Right,BackColor=Color.White,FlatStyle=FlatStyle.Flat,Margin=new Padding(0,8,0,0)};
        close.Click+=(_,_)=>Close();layout.Controls.Add(close,0,2);CancelButton=close;Controls.Add(layout);
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // Keep the bounded log buffer for the next opening, without an active window.
        log.Parent?.Controls.Remove(log);
        base.OnFormClosed(e);
    }
}
