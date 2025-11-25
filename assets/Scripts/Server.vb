Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Text.RegularExpressions

Public Module Server
    Public ReadOnly root As HNServer = New HNServer()

    Public Class HNServer
        Public Sub New()
            IP = "root"
            Name = "Not Connected"
            OpenPorts = New Integer() {80, 25, 21, 22}
            NeedCrackPortsCount = 0
            HasProxy = False
            HasFirewall = False
            HasTrace = False
            TraceSpeed = 1.0F
            ConnectedServers = New String() {}
            Contents = Nothing
            ContentsRaw = ""
            UserName = ""
            Password = ""
            CanLogin = False
            
        End Sub
        Public Property CrackedPorts As Integer
        Public Property IP As String
        Public Property Name As String
        Public Property OpenPorts() As Integer()
        Public Property NeedCrackPortsCount As Integer
        Public Property HasProxy As Boolean
        Public Property HasFirewall As Boolean
        Public Property HasTrace As Boolean
        Public Property TraceSpeed As Single
        Public Property ConnectedServers() As String()
        Public Property IsAdmin As Boolean
        Public Property UserName As String
        Public Property Password As String
        Public Property CanLogin As Boolean

        ' 真正的檔案系統物件（若暫時不解析，可為 Nothing）
        Public Property Contents As Entropy.System.FileSys
        ' 儲存原始 contents JSON 字串，之後再解析成 FileSys
        Public Property ContentsRaw As String
        Public Sub Check()
            If IP = "116.121.4.6" Then
                IsAdmin = True
            End If
        End Sub
        Public Sub ScanServer()
            If Not IsAdmin Then
                Console.WriteLine("Admin permission is required for scan.")
                Return
            End If
            Console.WriteLine("Scanning: " & Name & " (" & IP & ")")
            If ConnectedServers Is Nothing OrElse ConnectedServers.Length = 0 Then
                Console.WriteLine("  No connected servers.")
            Else
                Console.WriteLine("  Connected servers:")
                For Each cs As String In ConnectedServers
                    Console.WriteLine("   - " & cs)
                Next
            End If
        End Sub

        Public Function Login(userName As String, password As String) As Boolean
            If CanLogin AndAlso UserName = userName AndAlso Password = password Then
                IsAdmin = True
                Return True
            Else
                Return False
            End If
        End Function
        Public Function GetFileExists(DirN As String, Fname As String) As Boolean
            Return Contents.FileExist(DirN, Fname)
        End Function
    End Class
    Public Class ServerInfoParse
        ' 以手寫解析（不使用 JsonLib），改用 brace-depth 抽取頂層 object
        Public Function LoadServers(path As String) As List(Of HNServer)
            Dim result As New List(Of HNServer)()
            If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then
                Return result
            End If

            Dim txt As String = File.ReadAllText(path)

            ' 先以 brace-depth 擷取每一個頂層 object（容錯巢狀物件）
            Dim objects As New List(Of String)()
            Dim sb As New System.Text.StringBuilder()
            Dim depth As Integer = 0
            For i As Integer = 0 To txt.Length - 1
                Dim ch As Char = txt(i)
                If ch = "{"c Then
                    depth += 1
                End If
                If depth > 0 Then
                    sb.Append(ch)
                End If
                If ch = "}"c Then
                    depth -= 1
                    If depth = 0 Then
                        objects.Add(sb.ToString())
                        sb.Clear()
                    End If
                End If
            Next

            For Each obj As String In objects
                Dim body As String = obj
                ' 移除外層大括號，讓後續 Regex 針對 body 內部解析更直覺
                If body.StartsWith("{"c) AndAlso body.EndsWith("}"c) Then
                    body = body.Substring(1, body.Length - 2)
                End If

                Dim s As New HNServer()

                Dim mName = Regex.Match(body, """name""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase)
                If mName.Success Then s.Name = mName.Groups(1).Value

                Dim mIp = Regex.Match(body, """ip""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase)
                If mIp.Success Then s.IP = mIp.Groups(1).Value

                Dim mProxy = Regex.Match(body, """proxy""\s*:\s*(true|false)", RegexOptions.IgnoreCase)
                If mProxy.Success Then s.HasProxy = (mProxy.Groups(1).Value.ToLower() = "true")

                Dim mFirewall = Regex.Match(body, """firewall""\s*:\s*(true|false)", RegexOptions.IgnoreCase)
                If mFirewall.Success Then s.HasFirewall = (mFirewall.Groups(1).Value.ToLower() = "true")

                Dim mNeed = Regex.Match(body, """need_crack_ports""\s*:\s*(\d+)", RegexOptions.IgnoreCase)
                If mNeed.Success Then
                    Dim tmp As Integer = 0
                    Integer.TryParse(mNeed.Groups(1).Value, tmp)
                    s.NeedCrackPortsCount = tmp
                End If

                ' 解析 ports 陣列（如果有）
                Dim mPorts = Regex.Match(body, """ports""\s*:\s*\[([^\]]*)\]", RegexOptions.IgnoreCase)
                If mPorts.Success Then
                    Dim inner = mPorts.Groups(1).Value
                    Dim nums As New List(Of Integer)()
                    For Each numM As Match In Regex.Matches(inner, "-?\d+")
                        Dim v As Integer
                        If Integer.TryParse(numM.Value, v) Then nums.Add(v)
                    Next
                    If nums.Count > 0 Then s.OpenPorts = nums.ToArray()
                End If

                ' 解析 userName / password / canLogin
                Dim mUserName = Regex.Match(body, """userName""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase)
                If mUserName.Success Then s.UserName = mUserName.Groups(1).Value

                Dim mPassword = Regex.Match(body, """password""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase)
                If mPassword.Success Then s.Password = mPassword.Groups(1).Value

                Dim mCanLogin = Regex.Match(body, """canLogin""\s*:\s*(true|false)", RegexOptions.IgnoreCase)
                If mCanLogin.Success Then s.CanLogin = (mCanLogin.Groups(1).Value.ToLower() = "true")

                ' 解析 connected_servers 陣列（字串）
                Dim mCs = Regex.Match(body, """connected_servers""\s*:\s*\[([^\]]*)\]", RegexOptions.IgnoreCase)
                If mCs.Success Then
                    Dim inner = mCs.Groups(1).Value
                    Dim listCs As New List(Of String)()
                    For Each strM As Match In Regex.Matches(inner, """([^""]+)""")
                        listCs.Add(strM.Groups(1).Value)
                    Next
                    s.ConnectedServers = listCs.ToArray()
                Else
                    s.ConnectedServers = New String() {}
                End If

                ' 解析 contents：使用 regex 找 key，再檢查冒號後第一個非空白字元以決定解析方式
                Dim mContentsKey As Match = Regex.Match(body, """contents""\s*:\s*", RegexOptions.IgnoreCase)
                If mContentsKey.Success Then
                    Dim pos As Integer = mContentsKey.Index + mContentsKey.Length
                    ' 跳過空白
                    While pos < body.Length AndAlso Char.IsWhiteSpace(body(pos))
                        pos += 1
                    End While

                    If pos < body.Length Then
                        Dim ch As Char = body(pos)
                        If ch = "{"c Then
                            ' brace-depth 解析物件（避免與外層變數名稱衝突）
                            Dim depth2 As Integer = 0
                            Dim endIdx As Integer = -1
                            For j As Integer = pos To body.Length - 1
                                Dim c As Char = body(j)
                                If c = "{"c Then depth2 += 1
                                If c = "}"c Then
                                    depth2 -= 1
                                    If depth2 = 0 Then
                                        endIdx = j
                                        Exit For
                                    End If
                                End If
                            Next
                            If endIdx >= 0 Then
                                s.ContentsRaw = body.Substring(pos, endIdx - pos + 1)
                            Else
                                s.ContentsRaw = body.Substring(pos) ' 容錯：到結尾
                            End If
                        ElseIf ch = """"c Then
                            ' 字串形式，處理 escape quotes
                            Dim k As Integer = pos + 1
                            Dim found As Integer = -1
                            While k < body.Length
                                If body(k) = """"c Then
                                    Dim backslashCount As Integer = 0
                                    Dim t As Integer = k - 1
                                    While t >= 0 AndAlso body(t) = "\"c
                                        backslashCount += 1
                                        t -= 1
                                    End While
                                    If backslashCount Mod 2 = 0 Then
                                        found = k
                                        Exit While
                                    End If
                                End If
                                k += 1
                            End While
                            If found >= 0 Then
                                s.ContentsRaw = body.Substring(pos, found - pos + 1)
                            Else
                                s.ContentsRaw = body.Substring(pos) ' 容錯
                            End If
                        Else
                            ' 其他情況（例如 null 或非標準格式），儲存到下一個逗號或結尾
                            Dim endPos As Integer = body.IndexOf(","c, pos)
                            If endPos = -1 Then endPos = body.Length
                            s.ContentsRaw = body.Substring(pos, endPos - pos).Trim()
                        End If
                    Else
                        s.ContentsRaw = ""
                    End If
                Else
                    s.ContentsRaw = ""
                End If


                ' 初始化 Contents 為空的 FileSys，避免後續 NullReference
                s.Contents = New Entropy.System.FileSys()

                ' 若 ContentsRaw 看起來像物件且包含 files 陣列，嘗試簡單解析並加入 FileSys
                If Not String.IsNullOrWhiteSpace(s.ContentsRaw) AndAlso s.ContentsRaw.Contains("""files""") Then
                    Dim filesSection As Match = Regex.Match(s.ContentsRaw, """files""\s*:\s*\[([^\]]*)\]", RegexOptions.Singleline)

                    If filesSection.Success Then
                        Dim filesInner As String = filesSection.Groups(1).Value

                        For Each fm As Match In Regex.Matches(filesInner, "\{(.*?)\}", RegexOptions.Singleline)
                            Dim fileBody As String = fm.Groups(1).Value
                            Dim mFileName As Match = Regex.Match(fileBody, """name""\s*:\s*""([^""]*)""", RegexOptions.Singleline)
                            Dim mFileContent As Match = Regex.Match(fileBody, """content""\s*:\s*""([^""]*)""", RegexOptions.Singleline)
                            Dim f As New Entropy.System.File()
                            If mFileName.Success Then f.Name = mFileName.Groups(1).Value
                            If mFileContent.Success Then f.Content = mFileContent.Groups(1).Value
                            s.Contents.Files.Add(f)
                        Next
                    End If
                End If
                ' is_admin
                Dim isAdminMatch As Match = Regex.Match(s.ContentsRaw, """is_admin""\s*:\s*(true|false)", RegexOptions.IgnoreCase)
                If isAdminMatch.Success Then
                    s.IsAdmin = Boolean.Parse(isAdminMatch.Groups(1).Value)
                Else
                    s.IsAdmin = False
                End If

                ' Debug: 印出載入結果，方便確認
                ' Console.WriteLine("Loaded server: " & s.Name & " (" & s.IP & "), files: " & s.Contents.Files.Count)
                s.Check()
                result.Add(s)
            Next

            Return result
        End Function
    End Class
End Module