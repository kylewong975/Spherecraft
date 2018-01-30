using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required when Using UI elements.
using UnityEngine.Windows.Speech;

public class build : MonoBehaviour
{
	public GameObject memoryObject; 
	public float reachDistance = 100; 
	public AudioSource source;
	public AudioClip actSound;
	public float volume = 1f;
    public Camera lead;
    public Text text;

	private Color highlightColor = Color.green;
	private Color originalColor = Color.white;
	private bool hitSomething = false;
   
	private RaycastHit hitInfo;
	private Renderer prevRend = null;

    private float lastSpawnTime;


    private DictationRecognizer m_DictationRecognizer;




    #region helperfunctions 
    void CreateObject(Texture2D tex){
		print ("creating object");
		Transform tf = GetComponent<Transform>();
		GameObject mem= Instantiate(memoryObject, tf.position+transform.forward*2, Quaternion.identity);

		mem.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
		mem.GetComponent<Renderer>().material.mainTexture = tex;
	}

	IEnumerator CreateObjectWithImage(string url){
		print ("Changing object image");
		Texture2D tex;
		tex = new Texture2D (4, 4, TextureFormat.DXT1, false);
		using (WWW www = new WWW (url)) {
			yield return www;
			www.LoadImageIntoTexture (tex);
			CreateObject (tex);
			print ("Done");
		}
	}

	IEnumerator TextToImgURL(string text){
        text = text.Replace(" ", "%20");
		string url = "https://www.googleapis.com/customsearch/v1?key=AIzaSyDaFMJe1bHKCsFmpXrWjiXuyLfOvDp8zHU&cx=017866399407790186023:c2tnqoadtnw&searchType=image&alt=json&num=1&q=" + text;
		print ("changing " + text + " to img url");
		using (WWW www = new WWW(url)){
			yield return www;

			string result = www.text;
			print (result);
			int sindex = result.IndexOf("\"link\"");
			//nope didn't get imag
			if (sindex < 0)
				yield return 0;

			else {
				sindex = sindex + 9;
				int eindex = result.IndexOf (",", sindex)-1;
				if (eindex != -1) {
					string imgURL= result.Substring (sindex, eindex - sindex);
					yield return CreateObjectWithImage(imgURL);
				}
			}
		}
	}

	void DeleteObject(){
		if (!hitSomething) {
			print ("No object selected");
		} else {
			print ("Destroying object");
			Destroy (hitInfo.collider.gameObject);
		}

	}

	void ExpandObject(){
		print ("expanding object");
		if (!hitSomething) {
			print ("No object selected");
			return;
		} else {
			Transform tfOther = hitInfo.collider.gameObject.GetComponent<Transform>();
			tfOther.localScale *= 2;
		}
				
	}

	void ShrinkObject(){
		print ("shrinking object");
		if (!hitSomething) {
			print ("No object selected");
			return;
		} else {
			Transform tfOther = hitInfo.collider.gameObject.GetComponent<Transform>();
			tfOther.localScale *= .5F;
		}
	}

	void highlightSelected(){
		
		if (hitSomething)
		{
			Renderer currRend = hitInfo.collider.gameObject.GetComponent<Renderer>();
			//Transform tfOther = hitInfo.collider.gameObject.GetComponent<Transform>();
			//Draws ray in scene view during playmode; the multiplication in the second parameter controls how long the line will be
			//Debug.DrawLine(this.transform.position, tfOther.position, Color.blue);
			if (currRend!=null)
			{
				if(prevRend!=null)
					prevRend.material.color = originalColor;
				currRend.material.color = highlightColor;
				prevRend = currRend;
			}

		}
		else
		{
			if (prevRend!=null) {
				prevRend.material.color = originalColor;
			}
		}                                
	}

	void checkRay()
	{
        
		//A raycast returns a true or false value
		//we  initiate raycast through the Physics class
		//out parameter is saying take collider information of the object we hit, and push it out and 
		//store is in the what I hit variable. The variable is empty by default, but once the raycast hits
		//any collider, it's going to take the information, and store it in whatIHit variable. 
		hitSomething = Physics.Raycast(lead.transform.position, lead.transform.forward, out hitInfo, reachDistance);
		if(hitSomething && hitInfo.collider.gameObject.tag != "memoryObject"){
			hitSomething = false;
		}

	}

    void responseToSpeech(string speech)
    {

            
        if (speech == "delete")
        {
            DeleteObject();
            source.PlayOneShot(actSound, volume);
        }
        else if (speech == "increase")
        {

            ExpandObject();
             source.PlayOneShot(actSound, volume);
        }
        else if (speech == "decrease")
        {
            ShrinkObject();
             source.PlayOneShot(actSound, volume);
        }
        else {
            //don't create repeatedly too quickly
            if (Time.timeSinceLevelLoad - lastSpawnTime < 3)
                return;
            int index = speech.IndexOf("make");
            if (index != -1 && speech.Length > 5)
            {
                string searchTerm = speech.Substring(index + 5, speech.Length - index - 5);
                text.text = "Detected: " + searchTerm;
                StartCoroutine(TextToImgURL(searchTerm));
                source.PlayOneShot(actSound, volume);
                lastSpawnTime = Time.timeSinceLevelLoad;
            }
        }
    }
    void recognizeSpeech()
    {
        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            
            Debug.LogFormat("Dictation result: {0}", text);
            responseToSpeech(text);
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            //Debug.LogFormat("Dictation hypothesis: {0}", text);
            //responseToSpeech(text);
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            //restart
            print("Restart dictation");
            m_DictationRecognizer.Start();
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.Start();
    }
    #endregion

    // Update is called once per frame
    void Update () {
        checkRay();
        highlightSelected();
    }

	void Start()
	{
        recognizeSpeech();
    }


}
