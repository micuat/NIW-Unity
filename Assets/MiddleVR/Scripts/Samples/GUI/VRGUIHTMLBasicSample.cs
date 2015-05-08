/* VRGUIHTMLBasicSample

 * MiddleVR
 * (c) MiddleVR
 */

using UnityEngine;

/*
 * 
 * Use with data/GUI/HTMLBasicSample/index.html in MiddleVR install directory
 * 
 */
[AddComponentMenu("MiddleVR/Samples/GUI/HTML Basic")]
public class VRGUIHTMLBasicSample : MonoBehaviour
{
    // Disable warning CS0414 "The private field 'XXX' is assigned but its value is never used"
    #pragma warning disable 0414

    private vrCommand m_MyCommand;

    private vrValue CommandHandler(vrValue iValue)
    {
        print("HTML Button was clicked");

        // Uncomment the following lines to have modify the HTML page in response !
        //vrWebView webView = GetComponent<VRWebView>().webView;
        //webView.ExecuteJavascript("AddResult('Button was clicked !')");

        return null;
    }

    protected void Start()
    {
        m_MyCommand = new vrCommand("MyCommand", CommandHandler);
    }
}
