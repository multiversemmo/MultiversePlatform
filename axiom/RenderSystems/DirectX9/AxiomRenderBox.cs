using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Axiom.Core;
using System.Collections;

namespace Axiom.RenderSystems.DirectX9 {
    public class AxiomRenderBox : PictureBox {
        protected bool resumeLayout = false;
        public bool OverrideCursor = true;
        protected override void WndProc(ref Message m) {
            switch (m.Msg) {
                case 0x00000020: // WM_SETCURSOR
                    if (OverrideCursor)
                    {
                        if (Root.Instance.RenderSystem != null)
                        {
                            Root.Instance.RenderSystem.RestoreCursor();
                            m.Result = new IntPtr(1); // return TRUE;
                            return;
                        }
                    }
                    break;
            }
            if (!DefaultForm.WndOverride(ref m))
            {
                base.WndProc(ref m);
            }
        }

        private void InitializeComponent() {
            this.SuspendLayout();
            this.ResumeLayout(resumeLayout);
        }
    }
}
