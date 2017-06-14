using System;
using System.Windows.Forms;

namespace ffs2play
{
    public partial class AnalyseurManager
    {
        private ListViewEx m_lvDonneesFS;

         delegate void RempliTableauCallback();

        private void RempliTableau()
        {
            if (m_lvDonneesFS.InvokeRequired)
            {
                RempliTableauCallback d = new RempliTableauCallback(RempliTableau);
                m_lvDonneesFS.Invoke(d);
            }
            else
            {
                // On rempli la ligne correspondante dans la listview
                foreach (ListViewItem Item in m_lvDonneesFS.Items)
                {
                    if (Item.SubItems.Count < 1) continue;
                    ListViewItem.ListViewSubItem Sub = Item.SubItems[1];
                    switch (Item.Text)
                    {
                        case "Titre Avion":
                            Sub.Text = GetLastState().Title;
                            break;
                        case "Type Avion":
                            Sub.Text = GetLastState().Type;
                            break;
                        case "Model Avion":
                            Sub.Text = GetLastState().Model;
                            break;
                        case "Catégorie":
                            Sub.Text = GetLastState().Category;
                            break;
                        case "Altitude Avion":
                            Sub.Text = string.Format("{0:0.} ft", GetLastState().Altitude);
                            break;
                        case "Altitude Sol":
                            Sub.Text = string.Format("{0:0.} ft", GetLastState().Altitude - GetLastState().AltitudeSol);
                            break;
                        case "Vario":
                            Sub.Text = string.Format("{0:0.} ft/min", GetLastState().Vario);
                            break;
                        case "Direction":
                            Sub.Text = string.Format("{0:0.} °", GetLastState().Heading);
                            break;
                        case "Delta Altitude":
                            Sub.Text = string.Format("{0:0.} ft", GetLastState().AltitudeSol);
                            break;
                        case "Facteur Temps":
                            Sub.Text = string.Format("{0:0}X", GetLastState().TimeFactor);
                            break;
                        case "Longitude":
                            Sub.Text = string.Format("{0:0.000}°", GetLastState().Longitude);
                            break;
                        case "Latitude":
                            Sub.Text = string.Format("{0:0.000}°", GetLastState().Latitude);
                            break;
                        case "Vitesse Sol":
                            Sub.Text = string.Format("{0:0.}Kts", GetLastState().GSpeed);
                            break;
                        case "Vitesse TAS":
                            Sub.Text = string.Format("{0:0.}Kts", GetLastState().TASSpeed);
                            break;
                        case "Carburant":
                            Sub.Text = string.Format("{0:0.}Lbs", GetLastState().Fuel);
                            break;
                        case "Cabrage Avion":
                            Sub.Text = string.Format("{0:0.0}°", GetLastState().Pitch);
                            break;
                        case "Roulis Avion":
                            Sub.Text = string.Format("{0:0.0}°", GetLastState().Bank);
                            break;
                        case "G Force":
                            Sub.Text = string.Format("{0:0.0} G", GetLastState().GForce);
                            break;
                        case "Poids Avion":
                            Sub.Text = string.Format("{0:0.}Lbs", GetLastState().PoidsAvion);
                            break;
                        case "Total Fuel Capacity":
                            Sub.Text = string.Format("{0:0} Lbs", GetLastState().TotalFuelCapacity);
                            break;
                        case "Vitesse du vent":
                            Sub.Text = string.Format("{0:0} Kts", GetLastState().AmbiantWindVelocity);
                            break;
                        case "Direction du vent":
                            Sub.Text = string.Format("{0:0} °", GetLastState().AmbiantWindDirection);
                            break;
                        case "Précipitation":
                            Sub.Text = string.Format("{0:0}", GetLastState().AmbiantPrecipState);
                            break;
                        case "Pression Atm":
                            Sub.Text = string.Format("{0:0} mBar", GetLastState().AltimeterSetting);
                            break;
                        case "Pression MSL":
                            Sub.Text = string.Format("{0:0} mBar", GetLastState().SeaLevelPressure);
                            break;
                        case "Profondeur":
                            Sub.Text = string.Format("{0:0.000}", GetLastState().ElevatorPos);
                            break;
                        case "Ailerons":
                            Sub.Text = string.Format("{0:0.000}", GetLastState().AileronPos);
                            break;
                        case "Derive":
                            Sub.Text = string.Format("{0:0.000}", GetLastState().RudderPos);
                            break;
                        case "Squawk":
                            Sub.Text = string.Format("{0:0}", Outils.ConvertToBinaryCodedDecimal(GetLastState().Squawk));
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
