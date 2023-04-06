using TMPro;
using UnityEngine;

public class ShowScannerState : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI stateText;
    [SerializeField]
    private CodeReaderManager codeReader;

    private void Start() {
        SetCodeReaderStateText();
    }

    public void SetCodeReaderStateText() {
        stateText.text = codeReader.GetCurrentState();
    }
}
