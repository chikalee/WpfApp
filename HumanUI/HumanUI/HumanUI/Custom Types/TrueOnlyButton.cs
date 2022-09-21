using System.Windows.Controls;
namespace HumanUI
{

    /// <summary>
    /// Dummy wrapper class extending Button so that event switches know which type to address
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Button" />
    public class TrueOnlyButton : Button
    {
        public TrueOnlyButton()
            : base()
        {

        }
    
    }
}
