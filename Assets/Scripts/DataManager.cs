using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* 
Problem 1:
Description
Given a list of people with their birth and end years (all between 1900 and 2000), find the year with the most number of people alive.
Code
Solve using a language of your choice and dataset of your own creation.
Submission
Please upload your code, dataset, and example of the program’s output to Bit Bucket or Github. Please include any graphs or charts created by your program.
*/
public class DataManager : MonoBehaviour {
	[Header("Settings")]
	public int people = 1000;		// how many people to use in creating the dataset
	public Vector2 limits = new Vector2(1900,2000);	// min,max years to use for dataset
	private Vector2[] lifespans;	// collection of each persons lifespan in the batch
	private int[] years;			// collection of each year
	private int yearRange;			// number of years being checked
	private RectTransform[] nodes;	// node collection

	[Header("Graphing")]
	public RectTransform graphPanel;// reference to panel transform graph is drawn within
	public Transform nodeParent;	// reference for parenting purposes for each year's node objects
	public GameObject nodePrefab;	// prefab to use for showing year/people nodes
	public Text highText;
	[Space(5f)]
	public Text maxPeople;			// object reference for naming/positioning
	public Text midPeople;			// object reference for naming/positioning
	public Text minPeople;			// object reference for naming/positioning
	[Space(5f)]
	public Text maxYear;			// object reference for naming/positioning
	public Text midYear;			// object reference for naming/positioning
	public Text minYear;			// object reference for naming/positioning
	// private variables for graphing
	float padding, width, height, xVal, yVal, x, y;

	private bool testing = false;

	#region MonoBehaviour Methods
	void Start() {
		// set year range
		yearRange = Mathf.FloorToInt(limits.y-limits.x);
		// create our node collection
		CreateNodes();
		// generate an initial dataset and graph at start
		StartCoroutine(Process());
	}

	void Update() {
		// start/cancel testing
		if (Input.GetKeyDown(KeyCode.T)) {
			if (!testing) {
				RunTest();
			} else {
				testing = false;
				Debug.Log("Cancelled testing.");
			}
		}
	}
	#endregion

	#region Initialization
	/// <summary>
	/// Creates the node collection.
	/// </summary>
	private void CreateNodes() {
		nodes = new RectTransform[yearRange];
		for (int i = 0; i < nodes.Length; i++) {
			GameObject go = Instantiate(nodePrefab) as GameObject;
			go.name = "Node " + i.ToString();
			RectTransform tr = go.GetComponent<RectTransform>();
			if (tr == null) {
				Debug.LogError("Node objects require a RectTransform component.");
				break;
			}
			nodes[i] = tr;
		}
	}

	/// <summary>
	/// Begins the automated testing procedure.
	/// </summary>
	private void RunTest() {
		Debug.Log("Beginning test run.");
		testing = true;
		StartCoroutine(Testing());
	}

	/// <summary>
	/// Automated procedure for continuously testing
	/// datasets.
	/// </summary>
	IEnumerator Testing() {
		while(testing) {
			StartCoroutine(Process());
			yield return new WaitForSeconds(0.5f);
		}
		Debug.Log("Testing complete.");
		yield return null;
	}

	/// <summary>
	/// Automated procedure through generation/display of data.
	/// </summary>
	IEnumerator Process() {
		GenerateDataset();
		yield return new WaitForEndOfFrame();
		GenerateResults();
		yield return new WaitForEndOfFrame();
		Graph();
		yield return new WaitForEndOfFrame();
	}
	#endregion

	#region Data Methods
	/// <summary>
	/// Generates the dataset to utilize.
	/// </summary>
	private void GenerateDataset() {
		lifespans = new Vector2[people];

		for (int i = 0; i < people; i++) {
			lifespans[i] = GetLifespan();
		}
	}

	/// <summary>
	/// Creates a random lifespan for a person.
	/// </summary>
	/// <returns>The person's birth(x)/end(y) dates.</returns>
	private Vector2 GetLifespan() {
		Vector2 span = new Vector2();

		span.x = Random.Range(0, yearRange-1);
		span.y = Random.Range((int)span.x, yearRange);

		return span;
	}

	/// <summary>
	/// Generates the results by incrementing each year a person is alive.
	/// </summary>
	private void GenerateResults() {
		years = new int[yearRange];
		int start = 0, end = 0;
		for (int i = 0; i < lifespans.Length; i++) {
			start = (int)lifespans[i].x;
			end = (int)lifespans[i].y;
			if (start < 0 || end >= yearRange) {
				Debug.LogError("Start: " + start.ToString() + ", End: " + end.ToString() + ", Lifespan: " + lifespans[i].ToString());
			}
			for (int j = start; j <= end; j++) {
				years[j] += 1;
			}
		}
	}

	/// <summary>
	/// Get the year with the most people alive.
	/// </summary>
	/// <returns>The year(x) and count(y).</returns>
	private Vector2 HighestYear() {
		int most = 0, year = 0;

		// iterate through years for highest person count
		for (int i = 0; i < years.Length; i++) {
			if (years[i] > most) {
				most = years[i];
				year = i;
			}
		}

		return new Vector2(year, most);
	}
	#endregion

	#region Graphing
	/// <summary>
	/// Organize and graph dataset.
	/// </summary>
	private void Graph() {
		// retrieve highest target node
		Vector2 data = HighestYear();

		// simple log output
		//Debug.Log("Year: " + (data.x+limits.x).ToString() + "\n" + "People Alive: " + (data.y).ToString());
		if (highText == null) return;
		highText.text = ("Year: " + (data.x+limits.x).ToString() + "\n" + "High: " + (data.y).ToString());

		// ensure objects are assigned in inspector
		if (graphPanel == null) return;

		// store values for graph setup
		padding = 10f;
		width = graphPanel.rect.width-(padding*2f);
		height = graphPanel.rect.height-(padding*2f);

		// setup space to use for node graphing
		Vector2 min = new Vector2(graphPanel.rect.xMin+padding, graphPanel.rect.yMin+padding);
		Vector2 max = new Vector2(graphPanel.rect.xMax-padding, graphPanel.rect.yMax-padding);

		// ensure objects are assigned
		if (nodeParent == null || nodes == null) return;
		// store x-axis position for aligning year ID
		float xPos = (min.x+max.x)*0.5f;
		// create new node points and place them on the graph
		for (int i = 0; i < years.Length; i++) {
			// light max year green
			nodes[i].GetComponent<Image>().color = (i == (int)data.x ? Color.green : Color.white);
			nodes[i].transform.SetParent(graphPanel.transform);

			// 0-1 values to determine placement in graph
			xVal = Mathf.Clamp01((float)i/(float)yearRange);
			yVal = Mathf.Clamp01(((float)years[i])/data.y);

			// 2D pixel positioning
			x = xPos = (width*xVal)+min.x;
			y = (height*yVal)+min.y;
			nodes[i].anchoredPosition = new Vector2(x, y);
			nodes[i].transform.SetParent(nodeParent);
		}

		// ensure objects are assigned
		if (minYear == null || midYear == null || maxYear == null) return;

		// position/label year text
		minYear.text = ((int)limits.x).ToString();
		minYear.rectTransform.anchoredPosition = new Vector2(min.x, minYear.rectTransform.anchoredPosition.y);
		midYear.text = ((int)((limits.x+limits.y)*0.5f)).ToString();
		//midYear.rectTransform.anchoredPosition = new Vector2(xPos, midYear.rectTransform.anchoredPosition.y);
		maxYear.text = ((int)limits.y).ToString();
		maxYear.rectTransform.anchoredPosition = new Vector2(max.x, maxYear.rectTransform.anchoredPosition.y);

		// ensure objects are assigned
		if (minPeople == null || midPeople == null || maxPeople == null) return;

		// position/label people count text
		minPeople.text = "0";
		minPeople.rectTransform.anchoredPosition = new Vector2(minPeople.rectTransform.anchoredPosition.x, min.y);
		midPeople.text = ((int)(data.y*0.5f)).ToString();
		maxPeople.text = ((int)data.y).ToString();
		maxPeople.rectTransform.anchoredPosition = new Vector2(maxPeople.rectTransform.anchoredPosition.x, max.y);
	}
	#endregion
}
