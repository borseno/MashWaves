using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class TapAndHold : MonoBehaviour
{
    readonly float yToAdd = 0.25f; // amount of y coords to add for each new mash
    bool isHeld;
    bool gameLost;
    bool positionIsBeingChanged;
    List<GameObject> mashPull;
    new GameObject camera;
    Task makeBiggerCoroutine;

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

        camera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
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

                    newMash.transform.position = new Vector3(latestPosition.x, latestPosition.y + yToAdd, latestPosition.z);
                    newMash.transform.localScale = new Vector3(0f, yToAdd, 0f);

                    mashPull.Add(Instantiate(newMash));

                    //latestPosition = newMash.transform.position;
                    camera.transform.position = camera.transform.position + new Vector3(0f, yToAdd, 0f);
                }
            }
            else if (!positionIsBeingChanged && mashPull.Count > 1)
            {
                if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x > 0)
                {
                    OnLosingTheGame();
                    return;
                }
                else if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x >= -0.5f)
                    OnPerfectMove();
            }
        }
    }
    void OnLosingTheGame()
    {
        CurrentMash.GetComponent<Renderer>().material.color = Color.red;
        camera.transform.position = camera.transform.position + new Vector3(0, 0, -4f);
        gameLost = true;
        Invoke("DestroyCurrentMash", 0.5f);
    }
    void DestroyCurrentMash()
    {
        Destroy(CurrentMash);
    }
    void OnPerfectMove()
    {
        Task.FinishedHandler[] wavesForPrevious = new Task.FinishedHandler[mashPull.Count - 1];
        int index = -9999;
        int reversedIndex = -9999;

        for (index = mashPull.Count - 2; index >= 0; index--)
        {
            reversedIndex = mashPull.Count - 2 - index;

            wavesForPrevious[reversedIndex] = (m) =>
            {
                Vector3 toSubstract = mashPull[index].transform.position + new Vector3(0.3f, 0, 0.3f) - (mashPull[index].transform.position * 0.8f);
                ChangePosition(mashPull[index], new Vector3(0.3f, 0, 0.3f), toSubstract);

                Debug.Log("Index: " + index.ToString() + 
                    " ReversedIndex: " + reversedIndex.ToString() + 
                    " wavesForPreviousLength: " + wavesForPrevious.Length.ToString() +
                    " mashPullCount: " + mashPull.Count.ToString());
            };
        }

        ChangePosition(CurrentMash, new Vector3(0.4f, 0, 0.4f), new Vector3(0.2f, 0, 0.2f), wavesForPrevious);
    }
    IEnumerator MakeBigger(GameObject gameObject, Vector3 value)
    {
        Vector3 valueToAddOnIteration = new Vector3(value.x / 10f, value.y / 10f, value.z / 10f);

        while ((value.x > float.Epsilon && valueToAddOnIteration.x > 0)
            || (value.x < float.Epsilon && valueToAddOnIteration.x < 0))
        {
            gameObject.transform.localScale += valueToAddOnIteration;
            value -= valueToAddOnIteration;
            yield return new WaitForSeconds(0.01f);
        }
    }
    void ChangePosition(GameObject toChange, Vector3 bigger, Vector3 smaller, params Task.FinishedHandler[] actions)
    {

        positionIsBeingChanged = true;

        makeBiggerCoroutine = new Task(MakeBigger(toChange, bigger));
        makeBiggerCoroutine.Finished +=
            delegate (bool manually)
            {
                new Task(MakeBigger(toChange, smaller * -1)).Finished += (m) => { positionIsBeingChanged = false; };
            };
        
        



        makeBiggerCoroutine.Start();

    }
}
