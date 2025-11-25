Imports System
Imports System.Collections.Generic

Namespace Entropy.System
    Public Class FileSys
        ' 根目錄下的資料夾與檔案
        Public Property Dirs As List(Of Dir)
        Public Property Files As List(Of File)
        Private Property Root As String() = New String() {"home","bin", "log","sys"}
        Private Property Sys As String() = New String() {"x-server.sys", "os-config.sys", "bootcfg.dll", "netcfgx.dll"}
        Public Sub New()
            Dirs = New List(Of Dir)()
            For i As Integer = 0 To 3
                Dirs.Add(New Dir(Root(i)))
            Next
            For i As Integer = 0 To 3
                Dirs(3).Files.Add(New File(Sys(i)))
            Next
            Files = New List(Of File)()
        End Sub
        Public Function GetIndexDFiB(Name As String) As Dir
            For i As Integer = 0 To Dirs.Count - 1
                If Dirs(i).Name = Name Then
                    Return Dirs(i)
                End If
            Next

            Return Nothing
        End Function
        Public Function FileExist(DirN As String, FileN As String)
            Dim targetDir As Dir
            Dim exist As Boolean = False
            For Each d As Dir In Dirs
                If d.Name = DirN Then
                    targetDir = d
                    For Each f As File In targetDir.Files
                        If f.Name = FileN Then
                            exist = True
                            Exit For
                        End If
                    Next
                    Exit For
                End If
            Next
            Return exist
        End Function
    End Class

    Public Class Dir
        Public Property Name As String
        Public Property Files As List(Of File)
        Public Property Dirs As List(Of Dir)

        Public Sub New(Name As String)
            Files = New List(Of File)()
            Dirs = New List(Of Dir)()
            Me.Name = Name
        End Sub

        Public Sub Delete(fileName As String)
            ' 從後往前移除以避免迭代中移除問題
            For i As Integer = Files.Count - 1 To 0 Step -1
                If Files(i).Name = fileName Then
                    Files.RemoveAt(i)
                    Exit For
                End If
            Next
        End Sub
    End Class

    Public Class [File]
        Public Property Name As String
        Public Property Size As Integer
        Public Property Content As String

        ' 無參建構子（供解析器或其他處建立空檔案時使用）
        Public Sub New()
            Name = ""
            Size = 0
            Content = ""
        End Sub

        ' 可接受名稱的建構子（保留相容性）
        Public Sub New(ByVal Name As String)
            Me.Name = Name
            Size = 0
            Content = ""
        End Sub
    End Class

    Public Class BFile
        Inherits [File]
        ' 二進位內容保存在 Bytes
        Public Property ContentBytes() As Byte()

        Public Sub New()
            ContentBytes = New Byte() {}
        End Sub
    End Class
End Namespace