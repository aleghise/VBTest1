Imports System.Text.RegularExpressions
Imports Renci.SshNet

Public Class Main

    Public OpAction As String = "DF"
    Dim IP As String() = {"10.0.24.96", "10.0.134.75", "10.0.22.27"}
    Dim Uname As String() = {"aleghise", "root", "root"}
    Dim Upass As String() = {"alex95", "Password01", "Password01"}
    Dim remoteDirectory As String = "/"

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged

    End Sub

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ListBox1.Items.Add("nagios torino")
        ListBox1.Items.Add("nagios cr")
        ListBox1.Items.Add("nagios corsico")
        ListBox1.SetSelected(0, True)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim result As SshCommand

        TextBox1.Text = ""

        If OpAction = "DF" Then
            Using ssh = New SshClient(IP(ListBox1.SelectedIndex), Uname(ListBox1.SelectedIndex), Upass(ListBox1.SelectedIndex))
                ssh.Connect()
                result = ssh.RunCommand("df -Ph")
                ssh.Disconnect()
            End Using
            Dim lines As String() = result.Result.Split(vbLf)
            For Each Linea As String In lines
                TextBox1.Text = TextBox1.Text + vbCrLf + Linea
            Next
            ProcessResults(result.Result)

        Else
            TextBox1.Text = "Works in progress..."


            'Dim localDirectory As String = "/"

            TVpopulate(remoteDirectory)

        End If

        LabStatus.Text = OpAction + ": Hai selezionato " + ListBox1.SelectedItem.ToString
    End Sub

    Private Sub TVpopulate(ByVal LocDir As String)
        Using sftp = New SftpClient(IP(ListBox1.SelectedIndex), Uname(ListBox1.SelectedIndex), Upass(ListBox1.SelectedIndex))
            sftp.Connect()
            Dim files = sftp.ListDirectory(LocDir)

            TreeVsftp.Nodes.Clear()

            For Each Linea In files
                Dim ParentNode = New TreeNode()
                ParentNode.Text = Linea.Name
                TreeVsftp.Nodes.Add(ParentNode)
                sftp.ChangeDirectory("//" + ParentNode.Text + "/")
                Dim files2 = sftp.ListDirectory("")
                For Each S2 In files2
                    sftp.ChangeDirectory("//" + ParentNode.Text + "/")
                    Dim ChildtNode = New TreeNode()
                    ChildtNode.Text = S2.Name
                    TreeVsftp.Nodes.Add(ChildtNode)
                    sftp.ChangeDirectory(".")
                Next
            Next

            sftp.Disconnect()
        End Using
    End Sub


    Private Sub TVdir(ByVal LocDir As String)
        Using sftp = New SftpClient(IP(ListBox1.SelectedIndex), Uname(ListBox1.SelectedIndex), Upass(ListBox1.SelectedIndex))
            sftp.Connect()
            Dim files = sftp.ListDirectory(LocDir)
            sftp.Disconnect()

            TreeVsftp.Nodes.Clear()
            Dim root = New TreeNode(ListBox1.SelectedItem.ToString)
            TreeVsftp.Nodes.Add(root)
            TreeVsftp.Nodes(0).Nodes.Add(New TreeNode("root"))
            For Each Linea In files
                Dim remoteFileName As String = Linea.Name
                'If (Not Linea.Name.StartsWith(".")) AndAlso ((Linea.LastWriteTime.Date = DateTime.Today)) Then
                If (Not Linea.Name.StartsWith(".")) Then
                    'TextBox1.Text = TextBox1.Text + vbCrLf + Linea.ToString

                    TextBox1.Text = TextBox1.Text + vbCrLf + Linea.Name + " | " + Linea.Attributes.IsDirectory.ToString + " | " + Linea.Length.ToString
                    TreeVsftp.Nodes(0).Nodes(0).Nodes.Add(New TreeNode(Linea.Name))
                End If

            Next
        End Using

    End Sub


    Private Sub ProcessResults(ByVal resultData As String)
        ProgressBar1.Value = 0
        ProgressBar2.Value = 0
        ProgressBar3.Value = 0
        ProgressBar4.Value = 0
        ProgressBar5.Value = 0
        ProgressBar6.Value = 0

        Dim lines As String() = resultData.Split(vbLf)
        Dim deviceLines As List(Of String) = lines.ToList().Where(Function(x) x.StartsWith("/dev")).ToList()
        Dim matchPattern As String = "^(/dev/.+)\s+([0-9].+)\s+([0-9].+)\s+([0-9].+)\s+([0-9].+)\s+/.*$"

        For Each deviceLine As String In deviceLines
            Dim res = Regex.Match(deviceLine, matchPattern)
            Dim deviceName As String = (res.Groups(1).Value.Replace("/dev/", String.Empty)).Trim()
            Dim percentUsed As Integer = Convert.ToInt32((res.Groups(5).Value.Replace("%", String.Empty)).Trim())

            Select Case deviceName
                Case "mapper/vg00-root"
                    ProgressBar1.Value = percentUsed
                Case "mapper/VolGroup00-LogVol00"
                    ProgressBar1.Value = percentUsed
                Case "sda1"
                    ProgressBar2.Value = percentUsed
                Case "sda2"
                    ProgressBar3.Value = percentUsed
                Case "sdb1"
                    ProgressBar4.Value = percentUsed
                Case "sdb2"
                    ProgressBar5.Value = percentUsed
                Case "sdc1"
                    ProgressBar6.Value = percentUsed
            End Select
        Next
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        End

    End Sub



    Private Sub DFToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DFToolStripMenuItem.Click
        OpAction = "DF"
    End Sub

    Private Sub SFTPToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SFTPToolStripMenuItem.Click
        OpAction = "SFTP"
    End Sub

    Private Sub OperationToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OperationToolStripMenuItem.Click

    End Sub

    Private Sub MenuStrip1_ItemClicked(sender As Object, e As ToolStripItemClickedEventArgs) Handles MenuStrip1.ItemClicked

    End Sub

    Private Sub TreeVsftp_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles TreeVsftp.AfterSelect
        remoteDirectory = remoteDirectory + TreeVsftp.SelectedNode.Text + "/"
        LabStatus.Text = OpAction + ": Hai selezionato " + remoteDirectory
        TVdir(remoteDirectory)
    End Sub


End Class
