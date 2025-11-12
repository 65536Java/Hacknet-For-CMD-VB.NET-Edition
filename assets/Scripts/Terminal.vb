Imports System
Imports System.Threading

Namespace Terminals
    Public Class Terminal
        Public Shared Function terminal(ServersAvailable As HNServer(), ByRef CurrentComputer As HNServer) As String
            Dim cmd As String = "None"
            Dim Args() As String = {}
            Dim temp As String
            Console.Write(CurrentComputer.IP & "@" & Game.CurrentPath & "> ")
            temp = Console.ReadLine()
            If String.IsNullOrWhiteSpace(temp) Then Return "None"

            Dim parts() As String = temp.Split(" "c)
            cmd = parts(0).ToLowerInvariant()

            If parts.Length > 1 Then
                ReDim Args(parts.Length - 2)
                For i As Integer = 1 To parts.Length - 1
                    Args(i - 1) = parts(i)
                Next
            End If

            If cmd = "connect" Then
                ' connect [IP]
                If Args.Length = 0 Then
                    Console.WriteLine("Usage: connect [IP]")
                    Return cmd
                End If
                Dim ip As String = Args(0)
                For Each s As HNServer In ServersAvailable
                    If s.IP = ip OrElse String.Equals(s.Name, ip, StringComparison.OrdinalIgnoreCase) Then
                        CurrentComputer = s
                        Console.WriteLine("Connected to " & s.Name)
                        Return cmd
                    End If
                Next
                Console.WriteLine("Could not connect to " & ip)
                Return cmd

            ElseIf cmd = "scan" Then
                CurrentComputer.ScanServer()
                Return cmd

            ElseIf cmd = "ls" OrElse cmd = "dir" Then
                ' 防護：若 Contents 為 Nothing，建立空的 FileSys
                If CurrentComputer.Contents Is Nothing Then
                    CurrentComputer.Contents = New Entropy.System.FileSys()
                End If

                If String.IsNullOrEmpty(Game.CurrentPath) Then
                    For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                        Console.WriteLine(d.Name & "/")
                    Next
                    For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                        Console.WriteLine(f.Name)
                    Next
                Else
                    ' 簡單實作：只列當前目錄 Files（若需更完整路徑解析再實作）
                    Dim foundDir As Entropy.System.Dir = Nothing
                    For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                        If d.Name = Game.CurrentPath Then
                            foundDir = d : Exit For
                        End If
                    Next
                    If foundDir IsNot Nothing Then
                        For Each d As Entropy.System.Dir In foundDir.Dirs
                            Console.WriteLine(d.Name & "/")
                        Next
                        For Each f As Entropy.System.File In foundDir.Files
                            Console.WriteLine(f.Name)
                        Next
                    Else
                        Console.WriteLine("No such directory: " & Game.CurrentPath)
                    End If
                End If
                Return cmd

            ElseIf cmd = "dc" OrElse cmd = "disconnect" Then
                Console.WriteLine("Disconnected.")
                CurrentComputer = Server.root
                Return cmd
            ElseIf cmd = "cd" Then
                If Args(0) = ".." Then
                    Game.CurrentPath = ""
                Else
                    Game.CurrentPath = Args(0)
                End If
                Return cmd
            ElseIf cmd = "probe" OrElse cmd = "nmap" Then
                Dim portType As String
                Console.WriteLine("Open ports: ")
                For Each port As Integer In CurrentComputer.OpenPorts
                    If port = 80 Then
                        portType = "HTTP WebServer"
                    ElseIf port = 25 Then
                        portType = "SMTP Mail Server"
                    ElseIf port = 21 Then
                        portType = "FTP Server"
                    ElseIf port = 22 Then
                        portType = "SSH Server"
                    Else
                        portType = "Unknown"
                    End If
                    Console.WriteLine(" - " & port & "  (" & portType & ")")
                Next
                
                Console.WriteLine("Open ports required for crack: " & CurrentComputer.NeedCrackPortsCount)
                Return cmd

            ElseIf cmd = "porthack" Then
                Entropy.System.Process.StartProcess(New Porthack(CurrentComputer), Game.GetMaxRam(), Game.GetUsedRam())
                Return cmd
            ElseIf cmd = "read"
                ' read <file> - 顯示檔案內容，支援 bin/config.txt 與當前目錄
                If Args.Length = 0 Then
                    Console.WriteLine("Usage: read <file>")
                    Return cmd
                End If
                Dim target As String = Args(0)
                target = target.Replace("\"c, "/"c)

                ' 防護：確保 Contents 已初始化
                If CurrentComputer.Contents Is Nothing Then
                    CurrentComputer.Contents = New Entropy.System.FileSys()
                End If

                Dim found As Entropy.System.File = Nothing

                ' 1) 若有當前路徑，先在當前目錄尋找
                If Not String.IsNullOrEmpty(Game.CurrentPath) Then
                    Dim curDir As Entropy.System.Dir = Nothing
                    For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                        If String.Equals(d.Name, Game.CurrentPath, StringComparison.OrdinalIgnoreCase) Then
                            curDir = d : Exit For
                        End If
                    Next
                    If curDir IsNot Nothing Then
                        For Each f As Entropy.System.File In curDir.Files
                            If String.Equals(f.Name, target, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(f.Name, Game.CurrentPath & "/" & target, StringComparison.OrdinalIgnoreCase) Then
                                found = f : Exit For
                            End If
                        Next
                    End If
                End If

                ' 2) 在根 Files 中尋找（支援儲存為完整路徑如 "bin/config.txt"）
                If found Is Nothing Then
                    For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                        If String.Equals(f.Name.Replace("\"c, "/"c), target, StringComparison.OrdinalIgnoreCase) OrElse f.Name.EndsWith("/" & target, StringComparison.OrdinalIgnoreCase) Then
                            found = f : Exit For
                        End If
                    Next
                End If

                ' 3) 若 target 包含目錄，嘗試完整路徑比對
                If found Is Nothing AndAlso target.Contains("/") Then
                    For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                        If String.Equals(f.Name.Replace("\"c, "/"c), target, StringComparison.OrdinalIgnoreCase) Then
                            found = f : Exit For
                        End If
                    Next
                End If

                If found IsNot Nothing Then
                    Console.WriteLine(found.Content)
                Else
                    Console.WriteLine("File not found: " & target)
                End If
                Return cmd
            ElseIf cmd = "rm" Then
                ' rm <pattern> - 支援萬用字元 * 與 ?
                If Args.Length = 0 Then
                    Console.WriteLine("Usage: rm <file|pattern>")
                    Return cmd
                End If

                Dim pattern As String = Args(0).Replace("\"c, "/"c).Trim()
                If String.IsNullOrEmpty(pattern) Then
                    Console.WriteLine("Invalid pattern")
                    Return cmd
                End If

                If CurrentComputer.Contents Is Nothing Then
                    CurrentComputer.Contents = New Entropy.System.FileSys()
                End If

                ' 轉為 regex（簡單方式，escape 後替換 \* -> .* 與 \? -> .）
                Dim esc As String = System.Text.RegularExpressions.Regex.Escape(pattern)
                esc = esc.Replace("\*", ".*").Replace("\?", ".")
                Dim rx As New System.Text.RegularExpressions.Regex("^" & esc & "$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

                Dim deletedCount As Integer = 0

                ' 若 pattern 包含路徑分隔，直接比對完整路徑（檔案儲存名稱可能為 "bin/config.txt"）
                If pattern.Contains("/") Then
                    For i As Integer = CurrentComputer.Contents.Files.Count - 1 To 0 Step -1
                        Dim fname As String = CurrentComputer.Contents.Files(i).Name.Replace("\"c, "/"c)
                        If rx.IsMatch(fname) OrElse rx.IsMatch(System.IO.Path.GetFileName(fname)) Then
                            CurrentComputer.Contents.Files.RemoveAt(i)
                            deletedCount += 1
                        End If
                    Next
                Else
                    ' 刪除當前目錄內符合的檔案（如果有目錄結構）
                    If Not String.IsNullOrEmpty(Game.CurrentPath) Then
                        Dim curDir As Entropy.System.Dir = Nothing
                        For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                            If String.Equals(d.Name, Game.CurrentPath, StringComparison.OrdinalIgnoreCase) Then
                                curDir = d : Exit For
                            End If
                        Next
                        If curDir IsNot Nothing Then
                            For i As Integer = curDir.Files.Count - 1 To 0 Step -1
                                Dim fname As String = curDir.Files(i).Name.Replace("\"c, "/"c)
                                If rx.IsMatch(fname) OrElse rx.IsMatch(System.IO.Path.GetFileName(fname)) Then
                                    curDir.Files.RemoveAt(i)
                                    deletedCount += 1
                                End If
                            Next
                        End If
                    End If

                    ' 再在根目錄 Files 刪除符合的
                    For i As Integer = CurrentComputer.Contents.Files.Count - 1 To 0 Step -1
                        Dim fname As String = CurrentComputer.Contents.Files(i).Name.Replace("\"c, "/"c)
                        If rx.IsMatch(fname) OrElse rx.IsMatch(System.IO.Path.GetFileName(fname)) Then
                            CurrentComputer.Contents.Files.RemoveAt(i)
                            deletedCount += 1
                        End If
                    Next
                End If

                If deletedCount > 0 Then
                    Console.WriteLine("Deleted " & deletedCount & " file(s)")
                Else
                    Console.WriteLine("File not found: " & pattern)
                End If

                Return cmd
            ElseIf cmd = "ps" Then
                ' Display running processes
                For Each proc As Entropy.System.Process In Game.Processes
                    Console.WriteLine("Process ID: " & proc.PID & ", RAM Usage: " & proc.needRam & ", Name: " & proc.Name)
                Next
                Console.WriteLine("Total Processes: " & Game.Processes.Count)
            ElseIf cmd = "kill" Then
                Try
                    If Args.Length = 0 Then
                        Console.WriteLine("Usage: kill <pid>")
                        Return cmd
                    End If

                    Dim pid As Integer
                    If Not Integer.TryParse(Args(0), pid) Then
                        Console.WriteLine("Invalid PID: " & Args(0))
                        Return cmd
                    End If

                    ' 防護：Game.Processes 可能為 Nothing
                    If Game.Processes Is Nothing OrElse Game.Processes.Count = 0 Then
                        Console.WriteLine("No processes running.")
                        Return cmd
                    End If

                    Dim target As Entropy.System.Process = Nothing
                    For Each p As Entropy.System.Process In Game.Processes
                        If p IsNot Nothing AndAlso p.PID = pid Then
                            target = p
                            Exit For
                        End If
                    Next

                    If target Is Nothing Then
                        Console.WriteLine("Could not find process: " & pid)
                        Return cmd
                    End If

                    ' 嘗試 kill 並從列表移除（Process.Kill 或 KillModule 取決於你的實作）
                    Try
                        target.Kill()
                    Catch ex As Exception
                    End Try

                    ' 移除已終止的 process（容錯）
                    For i As Integer = Game.Processes.Count - 1 To 0 Step -1
                        If Game.Processes(i) Is Nothing OrElse Game.Processes(i).PID = pid Then
                            Game.Processes.RemoveAt(i)
                            Exit For
                        End If
                    Next

                    Console.WriteLine("Killed process: " & pid)
                Catch ex As Exception
                    Console.WriteLine("ERROR running kill: " & ex.Message)
                    Console.WriteLine(ex.StackTrace)
                End Try

                Return cmd
            Else 
                Console.WriteLine("Unknown command: " & cmd)
            End If
            Return cmd
        End Function
    End Class
End Namespace