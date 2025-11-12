Imports System
Imports System.IO
Imports System.Media 
Imports System.Diagnostics
Imports System.Threading
Imports Audio
Imports Server
Imports Engine
Imports Engine.Security
Imports System.Collections.Generic
Imports Terminals
Public Module Game
    ' 將 Directory/player 提升到模組層級
    Dim BaseDir As String
    Private player As SoundPlayer
    Private playerName As String
    Public CurrentPath As String = ""
    Dim RAM As Integer = 761
    Dim UsedRAM As Integer = 0
    Public Processes As New List(Of Entropy.System.Process)
    Public CurrentComputer As HNServer
    Sub CreateDirectory(path As String)
        ' 確保基本資料夾存在
        If Not Directory.Exists(System.IO.Path.Combine(BaseDir, path)) Then
            Directory.CreateDirectory(System.IO.Path.Combine(BaseDir, path))
        End If
    End Sub
    Public Function GetMaxRam() As Integer
        Return RAM
    End Function
    Public Function GetUsedRam() As Integer
        Return UsedRAM
    End Function
    Sub CreateFile(path As String, content As String)
        ' 確保基本檔案存在
        If Not File.Exists(System.IO.Path.Combine(BaseDir, path)) Then
            File.WriteAllText(System.IO.Path.Combine(BaseDir, path), content)
        End If
    End Sub
    Public ServersAvailable As New List(Of HNServer)
    ' 新增: 定義設定 Rich Presence 狀態的函數 (使用 P/Invoke Wrapper)
    Public Sub SetDiscordStatus(details As String, Optional state As String = "Playing Hacknet CMD Basic", Optional largeText As String = "HACKNET OS")
        Try
            ' 創建新的 RichPresence 結構體 (來自 Engine 命名空間)
            Dim presence As New Engine.RichPresence()
            
            ' FIX: 截斷字串以確保長度不超過 RichPresence 結構體中的固定大小
            ' details 和 state 限制為 128，所以截取到 127
            presence.details = If(details.Length > 127, details.Substring(0, 127), details)
            presence.state = If(state.Length > 127, state.Substring(0, 127), state)
            
            ' largeImageText/Key 限制為 256
            presence.largeImageKey = "hacknet_logo" 
            presence.largeImageText = If(largeText.Length > 255, largeText.Substring(0, 255), largeText)
            
            ' 呼叫 P/Invoke 更新狀態
            Engine.DiscordRPC.UpdatePresence(presence)
            
        Catch ex As Exception
            ' 忽略 Discord RPC 失敗，防止程式崩潰
            Debug.WriteLine("Discord RPC Failed: " & ex.Message)
        End Try
    End Sub

    Public Function GetConnectedServer(server As HNServer) As String()
        Return server.ConnectedServers
    End Function

    Public Sub GameMain(Dir As String)
        Dim DiscordAppID As String = "1428378052223697007" 
        
        ' 儲存 BaseDir 到模組變數
        BaseDir = Dir
        
        ' *** Discord RPC 初始化 (使用 P/Invoke Wrapper) ***
        Try
            ' VB.NET 2012 嚴格要求所有參數都用括號包住
            Engine.DiscordRPC.Initialize(DiscordAppID, IntPtr.Zero, True, Nothing)
            
            ' 設定初始狀態 (主選單)
            SetDiscordStatus("In Main Menu")
        Catch ex As Exception
            Debug.WriteLine("Discord RPC Initialization Failed: " & ex.Message)
        End Try
        
        Console.Title = "Hacknet for cmd: Basic"
        Dim targetFont As String = "MS Gothic"
        Dim targetSize As Integer = 20
        ConsoleFont.SetFont(targetFont, targetSize)
        
        ' 宣告迴圈變數 (GameMain)
        Dim selected As Integer = 0
        Dim menus() As String 
        ' 修正陣列初始化語法
        menus = New String() {"Play", "Options", "Exit"}
        
        Dim pressedKey As ConsoleKeyInfo
        Dim selectedValue As String = "Null"
        
        ' 音效初始化
        player = AudioUtil.GetSoundPlayer(BaseDir, "assets\audios\bgm.wav")
        player.PlayLooping()
        
        While True
            ' *** 定期呼叫 Discord_RunCallbacks (P/Invoke) ***
            Try
                ' VB.NET 2012 嚴格要求無參數呼叫也要用括號
                Engine.DiscordRPC.RunCallbacks()
            Catch
                ' 忽略 RunCallbacks 失敗
            End Try
            
            Console.Clear() 
            Logo("Basic")
            selectedValue = Menu(selected, menus)
            pressedKey = Console.ReadKey(True)
            
            If pressedKey.Key = ConsoleKey.UpArrow Then
                If selected > 0 Then
                    selected -= 1
                Else
                    selected = menus.Length - 1
                End If
            ElseIf pressedKey.Key = ConsoleKey.DownArrow Then
                If selected < menus.Length - 1 Then 
                    selected += 1
                Else
                    selected = 0
                End If
            ElseIf pressedKey.Key = ConsoleKey.Enter Then
                If selectedValue = "Exit" Then
                    ' *** 關閉 Discord RPC (P/Invoke) ***
                    Try
                        Engine.DiscordRPC.Shutdown() 
                    Catch
                        ' 忽略 Shutdown 失敗
                    End Try
                    ' Environment.Exit 必須用括號
                    Environment.Exit(0)
                ElseIf selectedValue = "Play" Then 
                    LoginSettings()
                ElseIf selectedValue = "Options" Then 
                    AudioUtil.GetSoundPlayer(BaseDir,"assets\audios\erro.wav").PlaySync()
                    player.PlayLooping()
                Else
                    AudioUtil.GetSoundPlayer(BaseDir,"assets\audios\erro.wav").PlaySync()
                    player.PlayLooping()
                End If
            End If
        End While
    End Sub

    Public Sub LoginSettings()
        ' 宣告迴圈變數 (LoginSettings)
        Dim selected As Integer = 0 
        Dim pressedKey As ConsoleKeyInfo 
        Dim selectedValue As String = "Null" 
        
        Dim menus() As String 
        menus = New String() {"Login", "Register", "Back"}
        
        While True
            ' *** 定期呼叫 Discord_RunCallbacks (P/Invoke) ***
            Try
                Engine.DiscordRPC.RunCallbacks()
            Catch
                ' 忽略 RunCallbacks 失敗
            End Try

            Console.Clear() 
            Logo("Play")
            selectedValue = Menu(selected, menus)
            pressedKey = Console.ReadKey(True)
            
            If pressedKey.Key = ConsoleKey.UpArrow Then
                If selected > 0 Then
                    selected -= 1
                Else
                    selected = menus.Length - 1 
                End If
            ElseIf pressedKey.Key = ConsoleKey.DownArrow Then
                If selected < menus.Length - 1 Then 
                    selected += 1
                Else
                    selected = 0
                End If
            ElseIf pressedKey.Key = ConsoleKey.Enter Then
                If selectedValue = "Back" Then
                    Return
                ElseIf selectedValue = "Login" Then 
                    Login()
                ElseIf selectedValue = "Register" Then 
                    Register()
                Else
                    ' 使用 BaseDir 和模組級 player
                    AudioUtil.GetSoundPlayer(BaseDir,"assets\audios\erro.wav").PlaySync()
                    player.PlayLooping()
                End If
            End If
        End While
    End Sub
    Sub Login()
        Console.Clear()
        Logo("Login")
        Dim username As String
        Dim password As String
        Console.WriteLine("Enter your username:")
        username = Console.ReadLine()
        Console.WriteLine("Enter your password:")
        password = Console.ReadLine()
        Dim filePath As String = BaseDir & "\Data\Player\Accounts\" & username & "\info.linf"
        If File.Exists(filePath) Then
            Dim fileContent As String = File.ReadAllText(filePath)
            Dim storedUsername As String = ""
            Dim storedPassword As String = ""
            If fileContent.StartsWith("<username>") AndAlso fileContent.Contains("<password>") Then
                Dim unameStart As Integer = "<username>".Length
                Dim unameEnd As Integer = fileContent.IndexOf("<password>")
                storedUsername = fileContent.Substring(unameStart, unameEnd - unameStart)
                storedPassword = fileContent.Substring(unameEnd + "<password>".Length)
            End If
            If storedUsername = username AndAlso storedPassword = Security.ToSHA256(password) Then
                Console.WriteLine("Login successful!")
                playerName = username
                StartGame()
            Else
                Console.WriteLine("Login failed!")
            End If
        Else 
            Console.WriteLine("User does not exist!")
        End If
        Console.ReadKey()
    End Sub
    Sub Register()
        Console.Clear()
        Logo("Register")
        Dim username As String
        Dim password As String
        Console.WriteLine("Enter your username:")
        username = Console.ReadLine()
        Console.WriteLine("Enter your password:")
        password = Console.ReadLine()
        Try
            If HNFiles.DirExists(BaseDir & "\Data\Player\Accounts\" & username) Then
               Console.WriteLine("User already exists!")
            Else
               CreateDirectory(BaseDir & "\Data\Player\Accounts\" & username)
               HNFiles.WriteFile(BaseDir & "\Data\Player\Accounts\" & username & "\info.linf", "<username>" & username & "<password>" & Security.ToSHA256(password))
               HNFiles.WriteFile(BaseDir & "\Data\Player\Accounts\" & username & "\session.ses", "1")
               Console.WriteLine("Registration successful!")
            End If
        Catch ex As Exception
            ' 處理可能發生的錯誤，例如檔案不存在、權限不足等
            Console.WriteLine("An error occurred: " & ex.Message)
        End Try
        Console.ReadKey()
    End Sub

    Public Sub Logo(SubTitle As String)
        Console.WriteLine("██╗  ██╗  █████╗   █████╗  ██╗  ██╗ ███╗  ██╗ ███████╗ ████████╗")
        Console.WriteLine("██║  ██║ ██╔══██╗ ██╔══██╗ ██║ ██╔╝ ████╗ ██║ ██╔════╝ ╚══██╔══╝")
        Console.WriteLine("███████║ ███████║ ██║  ╚═╝ █████═╝  ██╔██╗██║ █████╗      ██║")
        Console.WriteLine("██╔══██║ ██╔══██║ ██║  ██╗ ██╔═██╗  ██║╚████║ ██╔══╝      ██║")
        Console.WriteLine("██║  ██║ ██║  ██║ ╚█████╔╝ ██║ ╚██╗ ██║ ╚███║ ███████╗    ██║")
        Console.WriteLine("╚═╝  ╚═╝ ╚═╝  ╚═╝  ╚════╝  ╚═╝  ╚═╝ ╚═╝  ╚══╝ ╚══════╝    ╚═╝")
        Console.WriteLine("                      " & SubTitle & "                               ")
    End Sub
    Sub StartGame()
        Dim sr As New StreamReader("Boot.txt")
        player.Stop()
        Dim line As String = ""
        Console.Clear()
        Do
            Try
                line = sr.ReadLine()
            Catch ex As Exception
                Console.WriteLine("Error reading file: " & ex.Message)
            End Try
            
            If line IsNot Nothing Then
                Console.WriteLine(line)
                If line = "\n" Then
                    Thread.Sleep(2480)
                End If
                If line = "" Then
                    Thread.Sleep(2480)
                End If
                Thread.Sleep(20)
            End If
        Loop Until line Is Nothing
        Thread.Sleep(3000)
        Console.Clear()
        If HNFiles.ReadFile(BaseDir & "\Data\Player\Accounts\" & playerName & "\session.ses") = "1" Then
            ConsoleUtil.PrintWithDelay("-14 DAY TIMER EXPIRED : INITIALIZING FAILSAFE-")
            ConsoleUtil.PrintWithDelay("-----------------------------------------------------")
            Console.WriteLine("")
            ConsoleUtil.PrintWithDelay("Hi.")
            Thread.Sleep(500)
            ConsoleUtil.PrintWithDelay("...")
            Thread.Sleep(500)
            ConsoleUtil.PrintWithDelay("This is strange... Stranger than I expected.")
            Console.WriteLine("")
            Thread.Sleep(500)
            player = AudioUtil.GetSoundPlayer(BaseDir, "assets\audios\Revolve.wav")
            player.PlayLooping()
            ConsoleUtil.PrintWithDelay("I guess I'm supposed to write this in past tense, though I hardly feel like admitting it's over.")
            Console.WriteLine("")
            Thread.Sleep(1000)
            ConsoleUtil.PrintWithDelay("My name is Bit, and if you're reading this, I'm already dead.")
            Thread.Sleep(1000)
            For i As Integer = 0 To 10
                Console.Clear()
                Thread.Sleep(RandomNumber(100, 250))
                Console.WriteLine("Initializing...")
            Next
            ConsoleUtil.PrintWithDelay("Loading Modules......Complete")
            ConsoleUtil.PrintWithDelay("Loading Nodes......Complete")
            ConsoleUtil.PrintWithDelay("Reticulating Splines......Complete")
            ConsoleUtil.PrintWithDelay("--Initialization Complete--")
            Thread.Sleep(550)
            Console.WriteLine("Launching Tutorial...")
            Thread.Sleep(1000)
            Dim tutorialProcess As Entropy.System.Process = New Tutorial()
            Entropy.System.Process.StartProcess(tutorialProcess, RAM, UsedRAM, True)
            player.Stop()
            player = AudioUtil.GetSoundPlayer(BaseDir, "assets\audios\out_run_the_wolves.wav")
            player.PlayLooping()
            While True
                Terminal.terminal(ServersAvailable.ToArray(), CurrentComputer)
            End While
        End If
    End Sub
    Function RandomNumber(min As Integer, max As Integer) As Integer
        Dim rand As New Random()
        Return rand.Next(min, max + 1)
    End Function
    Function Menu(selected As Integer, menus() As String) As String
        Dim index As Integer = 0
        Dim selectedValue As String = "None"
        For Each menuN As String In menus
            If index = selected Then
                Console.WriteLine("    ["+menuN+"]")
                selectedValue = menuN
            Else
                Console.WriteLine(" "+menuN)
            End If
            Console.WriteLine("")
            index += 1
        Next
        Return selectedValue
    End Function
End Module