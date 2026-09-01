using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Languages {Svenska, English, Suomi, Norsk };
public class Settings : MonoBehaviour
{
    public List<Category> SelectedCategories { get; set; }
    public int SelectedCategoriesBinary { get; set; }
    public static Settings Instance { get; private set; }

    public static Action<bool> OnReceiverChange;
    public static Action MessageCountChange;

    private bool receiver = true;
    public bool Receiver { get => receiver; set
        {            
            receiver = value;
            OnReceiverChange?.Invoke(value);
        }
    }

    // ESP Received - ESP Transmitted - Firebase Recewived - Firebase Transmitted
    private int[] messageCount = new int[4];
    public int[] MessageCount => messageCount;
    public void AddMessageCount(int type)
    {
        messageCount[type]++;
        MessageCountChange?.Invoke();
    }

    public int LanguageIndex { get; set; } = 1;
    public string Language => Enum.GetName(typeof(Languages), LanguageIndex);

    [Header("Answer Colors")]
    [SerializeField] public Color NeutralGreyColor;
    [SerializeField] public Color CorrectColor;
    [SerializeField] public Color WrongColor;

    private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;

        Debug.Log("Created Settings");

		// Initiate with all
		//AddAllCategories();
		//Debug.Log("Binary:"+ Convert.ToString(SelectedCategoriesBinary, 2));
	}

    internal void AddOrRemoveCategory(int binaryValue)
    {
		Debug.Log("Combining SelectedCategories:"+Convert.ToString(SelectedCategoriesBinary,2)+" XOR "+ Convert.ToString(binaryValue, 2));
        SelectedCategoriesBinary = SelectedCategoriesBinary ^ binaryValue;
    }

    internal void AddAllCategories() => SelectedCategoriesBinary = Enum.GetValues(typeof(Category)).Cast<int>().Sum();

    internal void SwapLanguage()
    {
        Debug.Log("Swapping Language");
        LanguageIndex += 1;
        if(LanguageIndex >= Enum.GetNames(typeof(Languages)).Length)
            LanguageIndex = 0;
        Debug.Log("Language = "+Language);

    }
}
