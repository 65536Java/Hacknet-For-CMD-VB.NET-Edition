Imports System
Imports System.Threading

Namespace Terminals
    Public Class Terminal
        Public Shared Function terminal(ServersAvailable As HNServer(), ByRef CurrentComputer As HNServer) As String
            Try
                If ServersAvailable Is Nothing Then ServersAvailable = New HNServer() {}
                If CurrentComputer Is Nothing Then CurrentComputer = Server.root

                ' Game 為 Module，不能檢查 Game Is Nothing；只確保 CurrentPath 有值
                If String.IsNullOrEmpty(Game.CurrentPath) Then Game.CurrentPath = ""

                Dim cmd As String = "None"
                Dim Args() As String = {}
                Dim temp As String = ""

                Dim ipForPrompt As String = If(CurrentComputer IsNot Nothing AndAlso Not String.IsNullOrEmpty(CurrentComputer.IP), CurrentComputer.IP, "root")
                Dim pathForPrompt As String = If(String.IsNullOrEmpty(Game.CurrentPath), "", Game.CurrentPath)
                Console.Write(ipForPrompt & "@" & pathForPrompt & "> ")

                temp = Console.ReadLine()
                If String.IsNullOrWhiteSpace(temp) Then Return "None"

                Dim parts() As String = temp.Split(" "c)
                cmd = parts(0).ToLowerInvariant()

                If parts.Length > 1 Then
                    ReDim Args(parts.Length - 2)
                    For i As Integer = 1 To parts.Length - 1
                        Args(i - 1) = parts(i)
                    Next
                Else
                    Args = New String() {}
                End If

                If cmd = "connect" Then
                    If Args.Length = 0 Then
                        Console.WriteLine("Usage: connect [IP]")
                        Return cmd
                    End If
                    Dim ip As String = Args(0)
                    For Each s As HNServer In ServersAvailable
                        If s IsNot Nothing AndAlso (s.IP = ip OrElse String.Equals(s.Name, ip, StringComparison.OrdinalIgnoreCase)) Then
                            CurrentComputer = s
                            Console.WriteLine("Connected to " & s.Name)
                            CurrentComputer.Contents.GetIndexDFiB("log").Files.Add(New Entropy.System.File("116.121.4.6 Connected"))
                            Return cmd
                        End If
                    Next
                    Console.WriteLine("Could not connect to " & ip)
                    Return cmd

                ElseIf cmd = "scan" Then
                    If Not CurrentComputer.IsAdmin Then
                        Console.WriteLine("Permission denied: Admin rights required to scan.")
                        Return cmd
                    End If
                    If CurrentComputer IsNot Nothing Then CurrentComputer.ScanServer()
                    Return cmd
                ElseIf cmd = "ls" OrElse cmd = "dir" Then
                    If Not CurrentComputer.IsAdmin Then
                        Console.WriteLine("Permission denied: Admin rights required to list directory contents.")
                        Return cmd
                    End If
                    If CurrentComputer Is Nothing Then
                        Console.WriteLine("No computer connected.")
                        Return cmd
                    End If
                    If CurrentComputer.Contents Is Nothing Then CurrentComputer.Contents = New Entropy.System.FileSys()

                    If String.IsNullOrEmpty(Game.CurrentPath) Then
                        For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                            Console.WriteLine(d.Name & "/")
                        Next
                        For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                            Console.WriteLine(f.Name)
                        Next
                    Else
                        Dim foundDir As Entropy.System.Dir = Nothing
                        For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                            If d IsNot Nothing AndAlso d.Name = Game.CurrentPath Then
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
                    CurrentComputer.Contents.GetIndexDFiB("log").Files.Add(New Entropy.System.File("116.121.4.6 Disconnected"))
                    Console.WriteLine("Disconnected.")
                    CurrentComputer = Server.root
                    Return cmd

                ElseIf cmd = "cd" Then
                    If Not CurrentComputer.IsAdmin Then
                        Console.WriteLine("Permission denied: Admin rights required to change directories.")
                        Return cmd
                    End If
                    If Args.Length = 0 Then
                        Console.WriteLine("Usage: cd <dir|..>")
                        Return cmd
                    End If
                    If Args(0) = ".." Then
                        Game.CurrentPath = ""
                    Else
                        If String.IsNullOrEmpty(Game.CurrentPath) Then
                            For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                                If d.Name = Args(0) Then
                                    Game.CurrentPath = Args(0)
                                    Return cmd
                                End If
                            Next
                        Else
                            Dim foundDir As Entropy.System.Dir = Nothing
                            For Each d As Entropy.System.Dir In CurrentComputer.Contents.Dirs
                                If d IsNot Nothing AndAlso d.Name = Game.CurrentPath Then
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
                    End If
                    Return cmd

                ElseIf cmd = "probe" OrElse cmd = "nmap" Then
                    If CurrentComputer Is Nothing Then
                        Console.WriteLine("No computer connected.")
                        Return cmd
                    End If
                    Dim portType As String
                    Console.WriteLine("Open ports: ")
                    If CurrentComputer.OpenPorts IsNot Nothing Then
                        For Each port As Integer In CurrentComputer.OpenPorts
                            If port = 80 Then
                                portType = "HTTP WebServer"
                            ElseIf port = 25 Then
                                portType = "SMTP Mail Server"
                            ElseIf port = 21 Then
                                portType = "FTP Server"
                            ElseIf port = 22 Then
                                portType = "SSH Server"
                            ElseIf port = 443 Then
                                portType = "HTTPS WebServer"
                            Else
                                portType = "Unknown"
                            End If
                            Console.WriteLine(" - " & port & "  (" & portType & ")")
                        Next
                    End If
                    Console.WriteLine("Open ports required for crack: " & CurrentComputer.NeedCrackPortsCount)
                    Return cmd

                ElseIf cmd = "porthack" Then
                    If CurrentComputer IsNot Nothing Then
                        Entropy.System.Process.StartProcess(New Porthack(CurrentComputer), Game.GetMaxRam(), Game.GetUsedRam(), True)
                    Else
                        Console.WriteLine("No computer connected.")
                    End If
                    Return cmd

                ElseIf cmd = "cat" Then
                    If Not CurrentComputer.IsAdmin Then
                        Console.WriteLine("Permission denied: Admin rights required to read files.")
                        Return cmd
                    End If
                    If Args.Length = 0 Then
                        Console.WriteLine("Usage: cat <file>")
                        Return cmd
                    End If
                    Dim target As String = Args(0).Replace("\"c, "/"c)

                    If CurrentComputer.Contents Is Nothing Then CurrentComputer.Contents = New Entropy.System.FileSys()

                    Dim found As Entropy.System.File = Nothing

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

                    If found Is Nothing Then
                        For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                            If String.Equals(f.Name.Replace("\"c, "/"c), target, StringComparison.OrdinalIgnoreCase) OrElse f.Name.EndsWith("/" & target, StringComparison.OrdinalIgnoreCase) Then
                                found = f : Exit For
                            End If
                        Next
                    End If

                    If found Is Nothing AndAlso target.Contains("/") Then
                        For Each f As Entropy.System.File In CurrentComputer.Contents.Files
                            If String.Equals(f.Name.Replace("\"c, "/"c), target, StringComparison.OrdinalIgnoreCase) Then
                                found = f : Exit For
                            End If
                        Next
                    End If

                    If found IsNot Nothing Then
                        CurrentComputer.Contents.GetIndexDFiB("log").Files.Add(New Entropy.System.File("116.121.4.6 ReadFile " & target))
                        Console.WriteLine(found.Content)
                    Else
                        Console.WriteLine("File not found: " & target)
                    End If
                    Return cmd

                ElseIf cmd = "rm" Then
                    If Not CurrentComputer.IsAdmin Then
                        Console.WriteLine("Permission denied: Admin rights required to remove files.")
                        Return cmd
                    End If

                    Dim pattern As String = Args(0).Replace("\"c, "/"c).Trim()
                    If String.IsNullOrEmpty(pattern) Then
                        Console.WriteLine("Invalid pattern")
                        Return cmd
                    End If

                    If CurrentComputer.Contents Is Nothing Then CurrentComputer.Contents = New Entropy.System.FileSys()

                    Dim esc As String = System.Text.RegularExpressions.Regex.Escape(pattern)
                    esc = esc.Replace("\*", ".*").Replace("\?", ".")
                    Dim rx As New System.Text.RegularExpressions.Regex("^" & esc & "$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)

                    Dim deletedCount As Integer = 0

                    If pattern.Contains("/") Then
                        For i As Integer = CurrentComputer.Contents.Files.Count - 1 To 0 Step -1
                            Dim fname As String = CurrentComputer.Contents.Files(i).Name.Replace("\"c, "/"c)
                            If rx.IsMatch(fname) OrElse rx.IsMatch(System.IO.Path.GetFileName(fname)) Then
                                CurrentComputer.Contents.Files.RemoveAt(i)
                                deletedCount += 1
                            End If
                        Next
                    Else
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
                        CurrentComputer.Contents.GetIndexDFiB("log").Files.Add(New Entropy.System.File("116.121.4.6 DeleteFile " & pattern))
                    Else
                        Console.WriteLine("File not found: " & pattern)
                    End If

                    Return cmd

                ElseIf cmd = "ps" Then
                    If Game.Processes Is Nothing OrElse Game.Processes.Count = 0 Then
                        Console.WriteLine("No processes running.")
                        Return cmd
                    End If
                    For Each proc As Entropy.System.Process In Game.Processes
                        If proc IsNot Nothing Then
                            Console.WriteLine("Process ID: " & proc.PID & ", RAM Usage: " & proc.needRam & ", Name: " & proc.Name)
                        End If
                    Next
                    Console.WriteLine("Total Processes: " & Game.Processes.Count)
                    Return cmd

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

                        If Game.Processes Is Nothing OrElse Game.Processes.Count = 0 Then
                            Console.WriteLine("No processes running.")
                            Return cmd
                        End If

                        Dim targetIdx As Integer = -1
                        For i As Integer = 0 To Game.Processes.Count - 1
                            Dim p = Game.Processes(i)
                            If p IsNot Nothing AndAlso p.PID = pid Then
                                targetIdx = i : Exit For
                            End If
                        Next

                        If targetIdx = -1 Then
                            Console.WriteLine("Could not find process: " & pid)
                            Return cmd
                        End If

                        Dim targetProc As Entropy.System.Process = Game.Processes(targetIdx)

                        Dim stopped As Boolean = False
                        Try
                            stopped = targetProc.SafeStop(1000)
                        Catch ex As Exception
                            stopped = False
                        End Try

                        If Not stopped Then
                            targetProc.Kill()
                            System.Threading.Thread.Sleep(200)
                        End If

                        For i As Integer = Game.Processes.Count - 1 To 0 Step -1
                            If Game.Processes(i) Is Nothing OrElse Game.Processes(i).PID = pid Then
                                Game.Processes.RemoveAt(i)
                                Exit For
                            End If
                        Next

                        Console.WriteLine("Killed process: " & pid)
                    Catch ex As Exception
                        Console.WriteLine("ERROR running kill: " & ex.Message)
                    End Try
                    Return cmd
                ElseIf String.Equals(cmd, "Forkbomb", StringComparison.OrdinalIgnoreCase) Then
                    Dim fb As New ForkBomb()
                    Entropy.System.Process.StartProcess(fb, Game.GetMaxRam(), Game.GetUsedRam())
                Else
                    Console.WriteLine("Unknown command: " & cmd)
                End If

                Return cmd

            Catch ex As Exception
                Console.WriteLine("Unhandled terminal error: " & ex.Message)
                Console.WriteLine(ex.StackTrace)
                Return "Error"
            End Try
        End Function
    End Class
End Namespace