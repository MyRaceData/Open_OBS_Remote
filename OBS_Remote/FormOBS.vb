Imports WebSocket4Net
Imports System.Web.Script.Serialization

Public Class FormOBS
    Public Shared MySocket As WebSocket = New WebSocket("ws://" & My.Settings.MyIP & ":" & My.Settings.MyPort)
    Public CurScene As String = ""
    Public CurProfile As String = ""

    Function CheckConnection() As Boolean
        Try
            AddHandler MySocket.Opened, Sub(s, e) MySocketOpened(s, e)
            AddHandler MySocket.Error, Sub(s, e) MySocketError(s, e)
            AddHandler MySocket.Closed, Sub(s, e) MySocketClosed(s, e)
            AddHandler MySocket.MessageReceived, Sub(s, e) MyTxtRecieved(s, e)
            AddHandler MySocket.DataReceived, Sub(s, e) MyDataReceived(s, e)
            If MySocket.State > 1 Or MySocket.State < 0 Then
                MySocket.Open()
            End If
            Do
                If MySocket.State = 1 Or MySocket.State = 3 Then Exit Do
            Loop
        Catch ex As Exception
            MessageBox.Show("Check Connection sub bombed" & vbCrLf & ex.Message)
            Return False
        End Try
        If MySocket.State = 1 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub FormOBS_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckForIllegalCrossThreadCalls = False
        ListView1.HeaderStyle = ColumnHeaderStyle.None
        If Not CheckConnection() Then
            MessageBox.Show("Connection could not be established" & vbCrLf & "Check that OBS is running on the target machine")
            My.Forms.Form1.Close()
            End
        End If
        My.Forms.Form1.Hide()
    End Sub

    Sub MySocketOpened(s As Object, e As EventArgs)
        MySocket.Send("{""request-type"":""GetVersion"",""message-id"":""startup""}")
        MySocket.Send("{""request-type"":""GetSceneList"",""message-id"":""GetScenes""}")
        MySocket.Send("{""request-type"":""GetCurrentProfile"",""message-id"":""GetProfile""}")
        MySocket.Send("{""request-type"":""GetCurrentScene"",""message-id"":""GetCurScene""}")
    End Sub

    Sub MySocketClosed(s As Object, e As EventArgs)
        MessageBox.Show("The connection to OBS was closed" & vbCrLf & "Check that OBS is running on the target machine")
        Close()
        End
    End Sub

    Sub MySocketError(s As Object, e As SuperSocket.ClientEngine.ErrorEventArgs)
        If e.Exception.Message <> "No connection could be made because the target machine actively refused it" Then MessageBox.Show("An socket error occured " & vbCrLf & e.Exception.Message)
    End Sub

    Sub MyDataReceived(ss As Object, e As DataReceivedEventArgs)
        TextBox1.AppendText(e.Data.ToString)
    End Sub

    Sub MyTxtRecieved(s As Object, e As MessageReceivedEventArgs)
        Dim MyJson As Object = New JavaScriptSerializer().Deserialize(Of Object)(e.Message)
        'TextBox2.AppendText(e.Message)'for testing
        Try
            Dim myMessage = MyJson("update-type")
            OBSupdate(e.Message)
            Exit Sub
        Catch ex As Exception
        End Try

        Try
            Dim myID = MyJson("message-id")
            Select Case myID
                Case "startup"
                    TextBox1.Text = "Connection open"
                    TextBox1.AppendText(vbCrLf & "OBS Studio version: " & MyJson("obs-studio-version") & " running")
                    Exit Sub
                Case "GetScenes"
                    Label2.Text = "Current Scene: " & MyJson("current-scene")
                    CurScene = MyJson("current-scene")
                    Dim x() As Object = MyJson("scenes")
                    For i As Integer = 0 To x.Count - 1
                        If x(i)("name") = MyJson("current-scene") Then
                            Dim z As Object = ListBox1.Items.Add(x(i)("name"))
                            ListBox1.SetSelected(z, True)
                        Else
                            ListBox1.Items.Add(x(i)("name"))
                        End If
                    Next
                    Exit Sub
                Case "GetCurScene"
                    Label2.Text = "Current Scene: " & MyJson("name")
                    CurScene = MyJson("name")
                    Dim y() As Object = MyJson("sources")
                    'ListBox2.Items.Clear()
                    ListView1.Items.Clear()
                    Dim isVisible As Integer
                    For q As Integer = 0 To y.Count - 1
                        Dim txt As String = y(q)("name")
                        If y(q)("render") = True Then isVisible = 0
                        If y(q)("render") = False Then isVisible = 1
                        Dim listItem As New ListViewItem(txt, isVisible)
                        ListView1.Items.Add(listItem)
                    Next
                Case "GetProfile"
                    Label4.Text = "Current Profile: " & MyJson("profile-name")
                    Me.CurProfile = MyJson("profile-name")
                    TextBox1.AppendText(vbCrLf & "The Profile Changed " & TimeOfDay & " new profile: " & MyJson("profile-name"))
            End Select
        Catch ex As Exception
            MsgBox("an error occured in MyTxtRecieved sub" & vbCrLf & ex.Message)
        End Try
    End Sub

    Sub OBSupdate(X As String)
        Dim MyJson As Object = New JavaScriptSerializer().Deserialize(Of Object)(X)
        Try
            Dim MyUpdate = MyJson("update-type")
            Select Case MyUpdate
                Case "SwitchScenes"
                    Label2.Text = "Current Scene: " & MyJson("scene-name")
                    CurScene = MyJson("scene-name")
                    TextBox1.AppendText(vbCrLf & "Scene Changed " & TimeOfDay & " new scene: " & MyJson("scene-name"))
                    ListBox1.SetSelected(ListBox1.FindStringExact(MyJson("scene-name")), True)
                    'the following triggers sources being updated
                    MySocket.Send("{""request-type"":""GetCurrentScene"",""message-id"":""GetCurScene""}")
                Case "StreamStatus"
                    Label3.Text = "KBs encoded: " & CStr(MyJson("kbits-per-sec"))
                    Label9.Text = "Strain %: " & CStr(MyJson("strain"))
                    Label8.Text = "Elapsed Time: " & CStr(MyJson("total-stream-time"))
                Case "StreamStarted"
                    Label6.BackColor = Color.Red
                Case "RecordingStarted"
                    Label7.BackColor = Color.Red
                Case "StreamStopped"
                    Label6.BackColor = Color.DarkRed
                Case "RecordingStopped"
                    Label7.BackColor = Color.DarkRed
                Case "Exiting"
                    'application closes if connection is lost
                Case "ProfileChanged"
                    MySocket.Send("{""request-type"":""GetCurrentProfile"",""message-id"":""GetProfile""}")
                Case "SceneItemVisibilityChanged"
                    TextBox1.AppendText(vbCrLf & "Item visibilty Changed " & TimeOfDay)
                    TextBox1.AppendText(vbCrLf & "item: " & CStr(MyJson("item-name")))
                    If MyJson("item-visible") Then
                        TextBox1.AppendText(" visible")
                    Else
                        TextBox1.AppendText(" hidden")
                    End If
                Case Else

            End Select
        Catch ex As Exception
            MsgBox("an error occured in OBSupdate sub")
        End Try
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        Dim NewScene As String = ListBox1.SelectedItem.ToString
        If ListBox1.SelectedItem.ToString <> CurScene Then
            MySocket.Send("{""request-type"":""SetCurrentScene"",""message-id"":""SetScenes"",""scene-name"":""" & NewScene & """ }")
            CurScene = NewScene
            'the following triggers sources being updated
            MySocket.Send("{""request-type"":""GetCurrentScene"",""message-id"":""GetCurScene""}")
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If MessageBox.Show("Do you want to exit?", "OBS Remote", MessageBoxButtons.YesNo) = DialogResult.Yes Then
            MySocket.Close()
            MySocket = Nothing
            Close()
            End
        End If
    End Sub

    Private Sub FormOBS_LocationChanged(sender As Object, e As EventArgs) Handles Me.LocationChanged
        My.Settings.FormObsTop = Top
        My.Settings.FormObsLeft = Left
    End Sub

    Private Sub ListView1_Click(sender As Object, e As EventArgs) Handles ListView1.Click
        Dim str As String = ListView1.FocusedItem.Text
        If ListView1.FocusedItem.ImageIndex = "0" Then
            HideItem(str)
        Else
            ShowItem(str)
        End If
    End Sub

    Private Sub HideItem(str As String)
        MySocket.Send("{""request-type"":""SetSourceRender"",""message-id"":""SetRender"",""render"":false,""source"":""" & str & """ }")
        MySocket.Send("{""request-type"":""GetCurrentScene"",""message-id"":""GetCurScene""}")
    End Sub

    Private Sub ShowItem(str As String)
        MySocket.Send("{""request-type"":""SetSourceRender"",""message-id"":""SetRender"",""render"":true,""source"":""" & str & """ }")
        MySocket.Send("{""request-type"":""GetCurrentScene"",""message-id"":""GetCurScene""}")
    End Sub
End Class
