' ...existing code...
Imports System
Imports System.Threading
Imports System.Collections.Generic

Namespace Entropy.System
    Public MustInherit Class Process
        Public Shared CurrentComputer As Server.HNServer = Nothing
        Public isActived As Boolean
        Public PID As Integer
        Public Name As String
        Public needRam As Integer

        Private procThread As Thread

        Public Sub New()
        End Sub

        ' 非阻塞：在背景執行緒啟動
        Public Sub Start()
            isActived = True
            procThread = New Thread(AddressOf Me.ProcessMain)
            procThread.IsBackground = True
            procThread.Start()
            If Game.Processes Is Nothing Then
                Game.Processes = New List(Of Entropy.System.Process)()
            End If
            Game.Processes.Add(Me)
        End Sub

        ' 阻塞：在呼叫執行緒上直接執行 ProcessMain（會阻塞呼叫者）
        Public Sub StartBlocking()
            isActived = True
            If Game.Processes Is Nothing Then
                Game.Processes = New List(Of Entropy.System.Process)()
            End If
            Game.Processes.Add(Me)
            Try
                ' 直接呼叫 ProcessMain，呼叫者會被阻塞直到此方法結束
                Me.ProcessMain()
            Finally
                ' 清理：從 process 列表移除自身
                Try
                    For i As Integer = Game.Processes.Count - 1 To 0 Step -1
                        If Game.Processes(i) Is Me Then
                            Game.Processes.RemoveAt(i)
                            Exit For
                        End If
                    Next
                Catch
                End Try
                isActived = False
            End Try
        End Sub

        ' 溫和停止（設定旗標，ProcessMain 應檢查此旗標以結束）
        Public Sub Kill()
            isActived = False
        End Sub

        ' 嘗試等待執行緒在 timeoutMs 毫秒內結束（若為阻塞模式呼叫者自行等待即可）
        Public Function SafeStop(timeoutMs As Integer) As Boolean
            Try
                isActived = False
                If procThread Is Nothing Then Return True
                If Not procThread.IsAlive Then Return True
                Return procThread.Join(timeoutMs)
            Catch ex As ThreadAbortException
                Return False
            Catch
                Return False
            End Try
        End Function

        Public ReadOnly Property IsAlive As Boolean
            Get
                Return procThread IsNot Nothing AndAlso procThread.IsAlive
            End Get
        End Property

        ' 全域啟動輔助：保留舊簽章，並新增 overload 支援 blocking 參數
        Public Shared Sub StartProcess(p As Process, maxRam As Integer, usedRam As Integer)
            StartProcess(p, maxRam, usedRam, False)
        End Sub

        Public Shared Sub StartProcess(p As Process, maxRam As Integer, usedRam As Integer, Optional blocking As Boolean = False)
            If p Is Nothing Then Return

            If p.needRam + usedRam > maxRam Then
                Console.WriteLine("Not enough RAM to start process: " & p.Name)
                Return
            End If

            ' 分配 PID（簡單策略：若已有清單則以 count+1）
            Dim nextPid As Integer = 1
            If Game.Processes IsNot Nothing Then nextPid = Game.Processes.Count + 1
            p.PID = nextPid

            If blocking Then
                p.StartBlocking()
            Else
                p.Start()
            End If
        End Sub

        Public MustOverride Sub ProcessMain()
    End Class
End Namespace
' ...existing code...