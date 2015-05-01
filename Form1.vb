Imports System.Data
Imports System.Data.OleDb
Imports System.Windows.Forms


Public Class Form1
    Dim conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=MP3Data.accdb;Jet OLEDB:Database Password=DMP246810")
    Dim Command As New OleDbCommand("Select * From MP3List", conn)
    Dim Mp3Path As String
    Dim PlayMode As String = "Normal"
    Dim NowPlayMp3 As Int16
    Dim FirstPlay As Boolean = True



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadDB()
        If PlayerListBox.Items.Count > 0 Then
            PlayerListBox.SelectedIndex = 0
        End If
        ToolTip1.SetToolTip(Me.GroupBox1, "导入文件夹")
    End Sub

    Private Sub PlayerListBox_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles PlayerListBox.MouseDoubleClick
        If My.Computer.FileSystem.GetName(AxWindowsMediaPlayer1.URL) <> PlayerListBox.SelectedItem Then
            PlayMode = Button_PlayMode.Text
            Mp3Play()
        End If
    End Sub

    Private Sub NotifyIcon1_MouseClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseClick
        If e.Button = Windows.Forms.MouseButtons.Left Then
            If Me.Visible Then
                Me.Hide()
            Else
                Me.Show()
            End If
        End If
    End Sub

    Private Sub Timer_PlayProgress_Tick(sender As Object, e As EventArgs) Handles Timer_PlayProgress.Tick
        If TrackBar_PlayProgress.Maximum <> 0 And Int(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition * 100) < TrackBar_PlayProgress.Maximum Then
            TrackBar_PlayProgress.Value = Int(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition * 100)
        End If
    End Sub

    Private Sub LoadDB()
        Command.CommandText = "Select * From MP3List"
        conn.Open()
        Dim reader As OleDbDataReader = Command.ExecuteReader
        Do While reader.Read()
            PlayerListBox.Items.Add(reader("Name"))
        Loop
        reader.Close()
        conn.Close()
        ReComboBox()
    End Sub

    Private Sub Mp3Play(Optional ByVal Mp3Index As Int16 = -1)
        Dim URL As String
        If Mp3Index = -1 Then
            URL = Replace(PlayerListBox.SelectedItem, "'", "''")
            Command.CommandText = "Select URL From MP3List Where Name = '" & URL & "'"
            conn.Open()
            Dim reader As OleDbDataReader = Command.ExecuteReader
            If reader.Read Then
                TrackBar_PlayProgress.Value = 0
                AxWindowsMediaPlayer1.URL = reader(0)
                AxWindowsMediaPlayer1.Ctlcontrols.play()
                NowPlayMp3 = PlayerListBox.SelectedIndex
                Button_Play.Text = "Pause"
            End If
            reader.Close()
            conn.Close()

        Else
            PlayerListBox.SelectedIndex = Mp3Index
            URL = Replace(PlayerListBox.SelectedItem, "'", "''")
            Command.CommandText = "Select URL From MP3List Where Name = '" & URL & "'"
            conn.Open()
            Dim reader As OleDbDataReader = Command.ExecuteReader
            If reader.Read Then
                TrackBar_PlayProgress.Value = 0
                AxWindowsMediaPlayer1.URL = reader(0)
                AxWindowsMediaPlayer1.Ctlcontrols.play()
                NowPlayMp3 = PlayerListBox.SelectedIndex
                Button_Play.Text = "Pause"
            End If
            reader.Close()
            conn.Close()
        End If
        FirstPlay = False
    End Sub

    Private Sub AxWindowsMediaPlayer1_PlayStateChange(sender As Object, e As AxWMPLib._WMPOCXEvents_PlayStateChangeEvent) Handles AxWindowsMediaPlayer1.PlayStateChange
        TrackBar_PlayProgress.Maximum = Int(AxWindowsMediaPlayer1.currentMedia.duration * 100)
        If e.newState = WMPLib.WMPPlayState.wmppsMediaEnded Then
            Timer1.Enabled = True
        End If

    End Sub

    Private Sub TrackBar_PlayProgress_MouseDown(sender As Object, e As MouseEventArgs) Handles TrackBar_PlayProgress.MouseDown
        Timer_PlayProgress.Enabled = False
    End Sub

    Private Sub TrackBar_PlayProgress_MouseUp(sender As Object, e As MouseEventArgs) Handles TrackBar_PlayProgress.MouseUp
        AxWindowsMediaPlayer1.Ctlcontrols.currentPosition = TrackBar_PlayProgress.Value / 100
        Timer_PlayProgress.Enabled = True
    End Sub

    Private Sub TrackBar_SoundV_Scroll(sender As Object, e As EventArgs) Handles TrackBar_SoundV.Scroll
        AxWindowsMediaPlayer1.settings.volume = TrackBar_SoundV.Value * 10
    End Sub

    Private Sub Button_Play_Click(sender As Object, e As EventArgs) Handles Button_Play.Click
        PlayMode = Button_PlayMode.Text
        If AxWindowsMediaPlayer1.playState <> WMPLib.WMPPlayState.wmppsPlaying Then
            If FirstPlay = True Then
                FirstPlay = False
                Mp3Play(PlayerListBox.SelectedIndex)
                Button_Play.Text = "Pause"
            Else
                If AxWindowsMediaPlayer1.playState = WMPLib.WMPPlayState.wmppsPaused Then
                    AxWindowsMediaPlayer1.Ctlcontrols.play()
                    Button_Play.Text = "Pause"
                Else
                    Mp3Play(PlayerListBox.SelectedIndex)
                    Button_Play.Text = "Pause"
                End If
            End If
        Else
                AxWindowsMediaPlayer1.Ctlcontrols.pause()
                Button_Play.Text = "Play"
        End If

    End Sub

    Private Sub Button_Next_Click(sender As Object, e As EventArgs) Handles Button_Next.Click
        PlayMode = Button_PlayMode.Text
        PlayModeSub()
        If PlayMode = "Normal" Then
            AxWindowsMediaPlayer1.Ctlcontrols.stop()
            Mp3Play()
        End If
    End Sub

    Private Sub PlayModeSub()
        AxWindowsMediaPlayer1.Ctlcontrols.stop()
        Select Case PlayMode
            Case "Random"
                RandomMode()
            Case "ListLoop"
                ListLoopMode()
            Case "SingleLoop"
                SingleLoopMode()
            Case "Normal"
                NormalMode()
            Case "End"
                EndMode()
        End Select
    End Sub

    Private Sub NormalMode()
        AxWindowsMediaPlayer1.Ctlcontrols.stop()
        Button_Play.Text = "Play"
    End Sub

    Private Sub ListLoopMode()
        If PlayerListBox.Items.Count = NowPlayMp3 + 1 Then
            PlayerListBox.SelectedIndex = 0
            Mp3Play(0)
        Else
            PlayerListBox.SelectedIndex = NowPlayMp3 + 1
            Mp3Play()
        End If
    End Sub

    Private Sub SingleLoopMode()
        PlayerListBox.SelectedIndex = NowPlayMp3
        Mp3Play()
    End Sub

    Private Sub RandomMode()
        Dim Temp As New System.Random
        Dim TempIndex As Integer
        Do
            TempIndex = Temp.Next(PlayerListBox.Items.Count)

        Loop Until TempIndex <> PlayerListBox.SelectedIndex
        PlayerListBox.SelectedIndex = TempIndex
        Mp3Play(TempIndex)
    End Sub

    Private Sub EndMode()
        AxWindowsMediaPlayer1.Ctlcontrols.stop()
        PlayMode = Button_Next.Text
        Button_Play.Text = "Play"
    End Sub

    Private Sub GroupBox1_MouseClick(sender As Object, e As MouseEventArgs) Handles GroupBox1.MouseClick
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            Dim Name, URL As String
            AxWindowsMediaPlayer1.Ctlcontrols.stop()
            AxWindowsMediaPlayer1.URL = ""
            PlayerListBox.Items.Clear()
            Command.CommandText = "Delete * From MP3List"
            conn.Open()
            Command.ExecuteNonQuery()
            Command.CommandText = "ALTER TABLE MP3List ALTER COLUMN [ID] COUNTER (1, 1) "
            Command.ExecuteNonQuery()
            For Each foundFile As String In My.Computer.FileSystem.GetFiles(FolderBrowserDialog1.SelectedPath, FileIO.SearchOption.SearchTopLevelOnly, "*.mp3")
                Name = Replace(My.Computer.FileSystem.GetName(foundFile), "'", "''")
                URL = Replace(foundFile, "'", "''")
                Command.CommandText = "INSERT INTO MP3List (Name,URL) VALUES ('" & Name & "','" & URL & "')"
                Command.ExecuteNonQuery()
                PlayerListBox.Items.Add(My.Computer.FileSystem.GetName(foundFile))
            Next
            conn.Close()
            ReComboBox()
            '查询
            'For Each foundFile As String In My.Computer.FileSystem.GetFiles(FolderBrowserDialog1.SelectedPath, FileIO.SearchOption.SearchTopLevelOnly, "*.mp3")
            '    If My.Computer.FileSystem.GetName(foundFile) = "02.寺門通 - お前の父ちゃんチョメチョメ.mp3" Then
            '        AxWindowsMediaPlayer1.URL = foundFile
            '        AxWindowsMediaPlayer1.Ctlcontrols.play()
            '    End If
            'Next
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button_PlayMode.Click
        Select Case Button_PlayMode.Text
            Case "Normal"
                Button_PlayMode.Text = "ListLoop"
                PlayMode = "ListLoop"
            Case "ListLoop"
                Button_PlayMode.Text = "SingleLoop"
                PlayMode = "SingleLoop"
            Case "SingleLoop"
                Button_PlayMode.Text = "Random"
                PlayMode = "Random"
            Case "Random"
                Button_PlayMode.Text = "Normal"
                PlayMode = "Normal"
        End Select
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.WindowState = FormWindowState.Normal
            Me.Hide()
        End If
    End Sub

    Private Sub Timer_StartPlay_Tick(sender As Object, e As EventArgs) Handles Timer_StartPlay.Tick
        If AxWindowsMediaPlayer1.playState <> WMPLib.WMPPlayState.wmppsPlaying And TimeOfDay = TimeValue(MaskedTextBox1.Text & ":00") Then
            Mp3Play()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Select Case Button2.Text
            Case "Set"
                MaskedTextBox1.ReadOnly = True
                Timer_StartPlay.Enabled = True
                Button2.Text = "Stop"
            Case "Stop"
                MaskedTextBox1.ReadOnly = False
                Timer_StartPlay.Enabled = False
                Button2.Text = "Set"
        End Select
    End Sub


    Private Sub Timer_EndPlay_Tick(sender As Object, e As EventArgs) Handles Timer_EndPlay.Tick
        If TimeOfDay = TimeValue(MaskedTextBox2.Text & ":00") Then
            AxWindowsMediaPlayer1.Ctlcontrols.stop()
            If ComboBox1.SelectedItem <> "" Then
                PlayMode = "End"
                Mp3Play(ComboBox1.SelectedIndex - 1)
            Else
                Button_Play.Text = "Play"
            End If
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Select Case Button3.Text
            Case "Set"
                MaskedTextBox2.ReadOnly = True
                Timer_EndPlay.Enabled = True
                Button3.Text = "Stop"
                ComboBox1.Enabled = False
            Case "Stop"
                MaskedTextBox2.ReadOnly = False
                Timer_EndPlay.Enabled = False
                Button3.Text = "Set"
                ComboBox1.Enabled = True
        End Select
    End Sub

    Private Sub Menu_Exit_Click(sender As Object, e As EventArgs) Handles Menu_Exit.Click
        Me.Close()
    End Sub

    Private Sub ReComboBox()
        ComboBox1.Items.Clear()
        ComboBox1.Items.Add("")
        For i As Integer = 0 To PlayerListBox.Items.Count - 1
            ComboBox1.Items.Add(PlayerListBox.Items(i))
        Next
    End Sub

    Private Sub Panel1_MouseClick(sender As Object, e As MouseEventArgs) Handles Panel1.MouseClick
        If Panel2.Visible Then
            Panel2.Visible = False
        Else
            Panel2.Visible = True
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        PlayModeSub()
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click
        Call Panel1_MouseClick(Nothing, Nothing)
    End Sub

    Private Sub TrackBar_PlayProgress_Scroll(sender As Object, e As EventArgs) Handles TrackBar_PlayProgress.Scroll

    End Sub
End Class
