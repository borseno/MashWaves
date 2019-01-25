using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class TapAndHold : MonoBehaviour
{
    readonly float yScale = 0.25f; // y scale for every mash
    bool isHeld;
    bool gameLost;
    bool scaleIsBeingChanged;
    List<GameObject> mashPull;
    List<GameObject> perfectMoveMashes;
    new GameObject camera;
    Queue<object[]> Parameters;
    
    GameObject CurrentMash { get { return mashPull.LastOrDefault(); } }
    GameObject PreviousMash
    {
        get
        {
            if (mashPull.Count > 1)
                return mashPull[mashPull.Count - 2];
            else
                return null;
        }
    }

    void Start()
    {
        mashPull = new List<GameObject>() { GameObject.FindGameObjectWithTag("FirstCylinder") };
        perfectMoveMashes = new List<GameObject>();
        Parameters = new Queue<object[]>();

        camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (scaleIsBeingChanged)
            return;

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            isHeld = false;

        if (gameLost)
        {
            if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !isHeld)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); // restart the game
            }
            else return;
        }
        else
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (isHeld)
                {
                    CurrentMash.transform.localScale += new Vector3(0.01f, 0, 0.01f);

                    if (CurrentMash.transform.localScale.x >= PreviousMash.transform.localScale.x * 1.1f) // x = z, no need to check z
                    {
                        OnLosingTheGame();
                    }
                }
                else
                {
                    isHeld = true;

                    GameObject newMash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                    Vector3 latestPosition = CurrentMash.transform.position;

                    newMash.transform.position = new Vector3(latestPosition.x, latestPosition.y + yScale * 2, latestPosition.z);
                    newMash.transform.localScale = new Vector3(0f, yScale, 0f);

                    mashPull.Add(Instantiate(newMash));

                    camera.transform.position = camera.transform.position + new Vector3(0f, yScale * 2, 0f);
                }
            }
            else if (!perfectMoveMashes.Contains(CurrentMash) && !scaleIsBeingChanged && mashPull.Count > 1)
            {
                if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x > 0 
                    || CurrentMash.transform.localScale.x < 0.1f)
                {
                    OnLosingTheGame();
                    return;
                }
                else if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x >= -0.05f)
                {
                    OnPerfectMove();
                }
            }
        }
    }
    void OnLosingTheGame()
    {
        CurrentMash.GetComponent<Renderer>().material.color = Color.red;
        camera.transform.position -= new Vector3(0, 0, mashPull.Count * yScale);
        gameLost = true;
        Invoke("DestroyCurrentMash", 0.5f);
    }
    void DestroyCurrentMash()
    {
        Destroy(CurrentMash);
    }
    void OnPerfectMove()
    {
        perfectMoveMashes.Add(CurrentMash);

        scaleIsBeingChanged = true;

        bool forLoopWontIterate = perfectMoveMashes.Contains(mashPull[mashPull.Count - 2]);

        ChangeSize(CurrentMash,
            CurrentMash.transform.localScale + new Vector3(0.4f, 0, 0.4f),
            (CurrentMash.transform.localScale + new Vector3(0.4f, 0, 0.4f)).x > 1 ? new Vector3(1, yScale, 1) :
            CurrentMash.transform.localScale + new Vector3(0.4f, 0, 0.4f) - new Vector3(0.2f, 0, 0.2f),
            0f, forLoopWontIterate);

        if (!forLoopWontIterate)
        {
            int startingPosition = mashPull.Count - 2;
            for (int i = startingPosition; i >= 0 && !perfectMoveMashes.Contains(mashPull[i]); i--)
            {
                ChangeSize(mashPull[i],
                    mashPull[i].transform.localScale + new Vector3(0.3f, 0, 0.3f),
                    new Vector3(mashPull[i].transform.localScale.x * 0.8f,
                    mashPull[i].transform.localScale.y, mashPull[i].transform.localScale.z * 0.8f),
                    (startingPosition - i) / 10f + 0.3f,
                    i == 0 || perfectMoveMashes.Contains(mashPull[i - 1]));
            }
        }
    }
    void ChangeSize(GameObject toChange, Vector3 first, Vector3 second, float delayTime = 0f, bool final = false)
    {
        Parameters.Enqueue(new object[] { toChange, first });
        Invoke("ChangeSize", 0.0f + delayTime);
        Parameters.Enqueue(new object[] { toChange, second });
        Invoke("ChangeSize", 0.1f + delayTime);

        if (final)
            Invoke("SetScaleIsBeingChangedToFalse", 0.1f + delayTime + 0.1f);
    }
    void ChangeSize()
    {
        object[] parameter = Parameters.Dequeue();

        GameObject toChange = (GameObject)parameter[0];
        Vector3 newScale = (Vector3)parameter[1];

        toChange.transform.localScale = newScale;
    }
    void SetScaleIsBeingChangedToFalse()
    {
        scaleIsBeingChanged = false;
    }
}
