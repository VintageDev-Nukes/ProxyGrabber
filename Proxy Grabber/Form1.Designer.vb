<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cbServices = New System.Windows.Forms.ComboBox()
        Me.btnGo = New System.Windows.Forms.Button()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.lbProxies = New System.Windows.Forms.ListBox()
        Me.lblProxies = New System.Windows.Forms.Label()
        Me.lblEstado = New System.Windows.Forms.Label()
        Me.btnCheck = New System.Windows.Forms.Button()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(48, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Servicio:"
        '
        'cbServices
        '
        Me.cbServices.FormattingEnabled = True
        Me.cbServices.Location = New System.Drawing.Point(66, 6)
        Me.cbServices.Name = "cbServices"
        Me.cbServices.Size = New System.Drawing.Size(110, 21)
        Me.cbServices.TabIndex = 1
        '
        'btnGo
        '
        Me.btnGo.Location = New System.Drawing.Point(182, 6)
        Me.btnGo.Name = "btnGo"
        Me.btnGo.Size = New System.Drawing.Size(90, 21)
        Me.btnGo.TabIndex = 2
        Me.btnGo.Text = "¡Comenzar!"
        Me.btnGo.UseVisualStyleBackColor = True
        '
        'btnExport
        '
        Me.btnExport.Location = New System.Drawing.Point(15, 208)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(125, 25)
        Me.btnExport.TabIndex = 3
        Me.btnExport.Text = "Exportar..."
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'lbProxies
        '
        Me.lbProxies.FormattingEnabled = True
        Me.lbProxies.Location = New System.Drawing.Point(12, 55)
        Me.lbProxies.Name = "lbProxies"
        Me.lbProxies.Size = New System.Drawing.Size(260, 147)
        Me.lbProxies.TabIndex = 4
        '
        'lblProxies
        '
        Me.lblProxies.Location = New System.Drawing.Point(12, 34)
        Me.lblProxies.Name = "lblProxies"
        Me.lblProxies.Size = New System.Drawing.Size(260, 18)
        Me.lblProxies.TabIndex = 5
        Me.lblProxies.Text = "Lista de proxies obtenidas"
        Me.lblProxies.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblEstado
        '
        Me.lblEstado.Location = New System.Drawing.Point(9, 236)
        Me.lblEstado.Name = "lblEstado"
        Me.lblEstado.Size = New System.Drawing.Size(263, 15)
        Me.lblEstado.TabIndex = 6
        Me.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'btnCheck
        '
        Me.btnCheck.Location = New System.Drawing.Point(146, 208)
        Me.btnCheck.Name = "btnCheck"
        Me.btnCheck.Size = New System.Drawing.Size(125, 25)
        Me.btnCheck.TabIndex = 7
        Me.btnCheck.Text = "Comprobar..."
        Me.btnCheck.UseVisualStyleBackColor = True
        '
        'SaveFileDialog1
        '
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 258)
        Me.Controls.Add(Me.btnCheck)
        Me.Controls.Add(Me.lblEstado)
        Me.Controls.Add(Me.lblProxies)
        Me.Controls.Add(Me.lbProxies)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.btnGo)
        Me.Controls.Add(Me.cbServices)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Form1"
        Me.Text = "Proxy Grabber"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cbServices As System.Windows.Forms.ComboBox
    Friend WithEvents btnGo As System.Windows.Forms.Button
    Friend WithEvents btnExport As System.Windows.Forms.Button
    Friend WithEvents lbProxies As System.Windows.Forms.ListBox
    Friend WithEvents lblProxies As System.Windows.Forms.Label
    Friend WithEvents lblEstado As System.Windows.Forms.Label
    Friend WithEvents btnCheck As System.Windows.Forms.Button
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog

End Class
