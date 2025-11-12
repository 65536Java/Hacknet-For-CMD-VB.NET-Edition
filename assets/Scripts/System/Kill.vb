Imports System.Diagnostics
Imports System.Collections.Generic
Imports System
Namespace Entropy.System
    Public Module ProcKiller
        Public Sub KillProcess(processId As Integer, processList As List(Of Entropy.System.Process))
            If processList Is Nothing Then Return
            For Each it As Entropy.System.Process In processList
                If it IsNot Nothing AndAlso it.PID = processId Then
                    Try
                        it.Kill()
                    Catch
                        ' 忽略個別錯誤
                    End Try
                    Exit For
                End If
            Next
        End Sub
    End Module
End Namespace
 