﻿Public Class ScriptCommandEditor

    Private m_controller As EditorController
    Private WithEvents m_currentEditor As ICommandEditor
    Private m_script As IEditableScript

    Private m_ifEditor As IfEditor
    Private m_elemEditor As ElementEditor

    Private m_readOnly As Boolean

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs)
    Public Event CloseButtonClicked()

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ShowCloseButton = False
    End Sub

    Public Property Controller() As EditorController
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public Sub ShowEditor(script As IEditableScript)
        Dim newEditor As Control = Nothing
        Dim newEditorKey As String = If(script Is Nothing, String.Empty, script.EditorName)
        Dim resetIfEditor As Boolean = True
        Dim resetElemEditor As Boolean = True

        If script IsNot Nothing Then
            If script.Type = ScriptType.If Then
                If m_ifEditor Is Nothing Then
                    m_ifEditor = New IfEditor
                    m_ifEditor.Controller = m_controller
                End If

                newEditor = m_ifEditor
                m_ifEditor.Populate(DirectCast(script, EditableIfScript))
                m_ifEditor.IsReadOnly = IsReadOnly

                resetIfEditor = False
            Else
                If m_elemEditor Is Nothing Then
                    m_elemEditor = New ElementEditor
                    m_elemEditor.Controller = m_controller
                End If

                newEditor = m_elemEditor
                m_elemEditor.Initialise(m_controller, m_controller.GetEditorDefinition(script.EditorName))
                Dim data As IEditorData = m_controller.GetScriptEditorData(script)
                data.ReadOnly = IsReadOnly
                m_elemEditor.Populate(data)

                resetElemEditor = False
            End If
        End If

        ' It is important to "depopulate" any unused editors as we don't want them responding to updates
        ' to scripts that we're not editing if the user presses the undo button.
        If resetIfEditor And m_ifEditor IsNot Nothing Then
            m_ifEditor.Populate(Nothing)
        End If

        If resetElemEditor And m_elemEditor IsNot Nothing Then
            m_elemEditor.Populate(Nothing)
        End If

        Dim newCommandEditor As ICommandEditor = DirectCast(newEditor, ICommandEditor)

        If newEditor IsNot Nothing Then
            newEditor.Parent = Me
            newEditor.Dock = DockStyle.Fill

            ' Send pnlButtons to the back when in pop-out mode so that the docking works correctly,
            ' and pnlButtons doesn't obsure the bottom of newEditor.
            If pnlButtons.Visible Then pnlButtons.SendToBack()

            If newEditor IsNot m_currentEditor Then
                newEditor.Visible = True
            End If
        End If

        If m_currentEditor IsNot newEditor And m_currentEditor IsNot Nothing Then
            DirectCast(m_currentEditor, Control).Visible = False
        End If

        m_currentEditor = newCommandEditor
        m_script = script
    End Sub

    Public Sub Save()
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
        End If
    End Sub

    Private Sub m_editor_Dirty(sender As Object, args As DataModifiedEventArgs) Handles m_currentEditor.Dirty
        Dim newArgs As New DataModifiedEventArgs(String.Empty, m_script.DisplayString(CInt(args.Attribute), args.NewValue))
        RaiseEvent Dirty(Me, newArgs)
    End Sub

    Public Sub UpdateField(index As Integer, newValue As Object)
        m_currentEditor.UpdateField(index.ToString(), newValue, True)
    End Sub

    Public Sub UpdateField(id As String, newValue As Object)
        m_currentEditor.UpdateField(id, newValue, True)
    End Sub

    Public Property ShowCloseButton() As Boolean
        Get
            Return pnlButtons.Visible
        End Get
        Set(value As Boolean)
            pnlButtons.Visible = value
        End Set
    End Property

    Private Sub cmdClose_Click(sender As System.Object, e As System.EventArgs) Handles cmdClose.Click
        RaiseEvent CloseButtonClicked()
    End Sub

    Public ReadOnly Property MinHeight As Integer
        Get
            Return m_currentEditor.MinHeight
        End Get
    End Property

    Public Property IsReadOnly As Boolean
        Get
            Return m_readOnly
        End Get
        Set(value As Boolean)
            m_readOnly = value
        End Set
    End Property
End Class
