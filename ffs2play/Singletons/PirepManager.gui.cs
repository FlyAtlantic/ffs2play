using System;
using System.IO;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using System.Xml;

namespace ffs2play
{
    public partial class PirepManager
    {
        private Button m_btnConnect;
        private RichTextBox m_rtbDecryptedMetar;
        private TextBox m_tbMetar;
        /// <summary>
        /// Event Click sur le bouton de connexion au serveur VMS
        /// En fonction de l'état de connexion, on lance la connexion
        /// Ou on déconnecte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConPirep_Click(object sender, EventArgs e)
        {
            m_btnConnect.Enabled = false;
            if (!m_bConnected)
            {
                dlgConServeur ConPirep = new dlgConServeur(true);
                if (ConPirep.DialogResult != DialogResult.OK) ConPirep.ShowDialog();
            }
            else
            {
                Disconnect();
            }
        }
    }
}
