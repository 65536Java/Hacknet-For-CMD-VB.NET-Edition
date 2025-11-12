Imports System.Threading
Imports System
Imports System.Random
Imports Entropy.System
Public Class Porthack
    Inherits Process
    Dim Computer As HNServer
    Public Sub New(server As HNServer)
        Name = "Porthack"
        Computer = server
        needRam = 246
    End Sub
    Public Overrides Sub ProcessMain()
        Console.WriteLine("Porthack initialized -- Running...")
        For i As Integer = 1 To 100
            Thread.Sleep(50)
            Console.WriteLine(RandomString(10) & "          " & RandomString(10))
        Next
        Computer.IsCracked = True
        Console.WriteLine("Porthack complete - Password Found.")
        Kill()
    End Sub
    Function RandomString(length As Integer) As String
            Dim random As New Random()
            Dim chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
            Dim result As New System.Text.StringBuilder(length)
            For i As Integer = 1 To length
                Dim index As Integer = random.Next(chars.Length)
                result.Append(chars(index))
            Next
            Return result.ToString()
        End Function
End Class