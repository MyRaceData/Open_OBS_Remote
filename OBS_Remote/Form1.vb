Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        My.Forms.FormOBS.Show()
    End Sub
    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        My.Settings.MyIP = TextBox1.Text
    End Sub
    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        My.Settings.MyPort = TextBox2.Text
    End Sub
    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged
        My.Settings.MyExe = TextBox3.Text
    End Sub
    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        Top = My.Settings.Form1Top
        Left = My.Settings.Form1Left
    End Sub
    Private Sub Form1_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged
        My.Settings.Form1Top = Top
        My.Settings.Form1Left = Left
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        My.Settings.MyExe = TextBox3.Text
        My.Settings.MyIP = TextBox1.Text
        My.Settings.MyPort = TextBox2.Text
    End Sub
End Class
