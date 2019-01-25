using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class TapAndHold : MonoBehaviour
{
    bool isHeld; // is mouse button being held
    bool gameLost;
    bool scaleIsBeingChanged; // bool for waves animation
    List<GameObject> mashPull;
    List<GameObject> perfectMoveMashes;
    Queue<object[]> parameters; // Mash and how to change its size parameters for ChangeSize() method
    new GameObject camera;
    Settings settings;

    GameObject CurrentMash
    {
        get
        {
            return mashPull.LastOrDefault();
        }
    }
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
        parameters = new Queue<object[]>();
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        settings = SettingsReader.Instance.Settings;
    }

    void Update()
    {
        if (scaleIsBeingChanged) // no actions allowed during animations
            return;

        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            isHeld = false;

        if (gameLost)
        {
            // after losing the game the player should stop 
            // holding his mouse button for a moment in order to click and restart the game
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
                    CurrentMash.transform.localScale += 
                        new Vector3(
                            settings.MashGrowSpeed, 
                            0, 
                            settings.MashGrowSpeed
                            );

                    if (CurrentMash.transform.localScale.x >= PreviousMash.transform.localScale.x) // x = z, no need to check z
                    {
                        OnLosingTheGame();
                    }
                }
                else
                {
                    isHeld = true; 

                    Vector3 latestPosition = CurrentMash.transform.position;

                    GameObject newMash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                    newMash.transform.position = new Vector3(
                        latestPosition.x, 
                        latestPosition.y + settings.MashScaleY * 2, 
                        latestPosition.z
                        );
                    newMash.transform.localScale = new Vector3(0f, settings.MashScaleY, 0f);

                    mashPull.Add(Instantiate(newMash));

                    camera.transform.position += new Vector3(0f, settings.MashScaleY * 2, 0f);
                }
            }// stopped holding mouse button
            else if (!perfectMoveMashes.Contains(CurrentMash) && !scaleIsBeingChanged && mashPull.Count > 1)
            {
                if (CurrentMash.transform.localScale.x < settings.MinMashScale)
                {
                    GameObject.Destroy(CurrentMash);
                    mashPull.RemoveAt(mashPull.Count - 1);
                    camera.transform.position -= new Vector3(0f, settings.MashScaleY * 2, 0f);
                }

                // Game over: current mash > previous mash || current mash size < min mash size
                if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x > 0)
                {
                    OnLosingTheGame();
                    return;
                } // Perfect move: previous mash size >= current mash size for max deviation
                else if ((CurrentMash.transform.localScale - PreviousMash.transform.localScale).x 
                    >= -settings.MaxDeviation)
                {
                    OnPerfectMove();
                }
            }
        }
    }
    void OnLosingTheGame()
    {
        gameLost = true;
        CurrentMash.GetComponent<Renderer>().material.color = Color.red;
        camera.transform.position -= new Vector3(0, 0, mashPull.Count * settings.MashScaleY * 2);
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
        bool changeOnlyCurrent = false; //perfectMoveMashes.Contains(mashPull[mashPull.Count - 2]);

        Vector3 currentMashFirstScale = 
            CurrentMash.transform.localScale + new Vector3(settings.CurrentMashScaleToAdd, 0, settings.CurrentMashScaleToAdd);
        Vector3 currentMashSecondScale = currentMashFirstScale.x > settings.MaxMashScale
            ? new Vector3(settings.MaxMashScale, settings.MashScaleY, settings.MaxMashScale)
            : currentMashFirstScale - new Vector3(settings.CurrentMashScaleToSubstract, 0, settings.CurrentMashScaleToSubstract);

        ChangeSize(CurrentMash, currentMashFirstScale, currentMashSecondScale, 0f, changeOnlyCurrent);

        if (!changeOnlyCurrent)
        {
            int startingPosition = mashPull.Count - 2;
            bool underSecondPerfectMash = false;
            for (int i = startingPosition; i >= 0; i--)
            {
                Vector3 otherMashFirstScale = 
                    mashPull[i].transform.localScale + new Vector3(
                        settings.OtherMashesScaleToAdd, 
                        0, 
                        settings.OtherMashesScaleToAdd
                        );
                Vector3 otherMashSecondScale = new Vector3(
                    mashPull[i].transform.localScale.x * settings.OtherMashesScaleToMultiplyBy,
                    mashPull[i].transform.localScale.y, 
                    mashPull[i].transform.localScale.z * settings.OtherMashesScaleToMultiplyBy
                    );

                ChangeSize(
                    mashPull[i], 
                    otherMashFirstScale, 
                    underSecondPerfectMash ? mashPull[i].transform.localScale : otherMashSecondScale,
                    (startingPosition - i) / 10f + 0.3f,
                    i == 0 || perfectMoveMashes.Contains(mashPull[i - 1])
                    );

                    underSecondPerfectMash = underSecondPerfectMash || perfectMoveMashes.Contains(mashPull[i]);
            }
        }
    }
    void ChangeSize(GameObject toChange, Vector3 first, Vector3 second, float delayTime = 0f, bool final = false)
    {
        parameters.Enqueue(new object[] { toChange, first });
        Invoke("ChangeSize", 0.0f + delayTime);
        parameters.Enqueue(new object[] { toChange, second });
        Invoke("ChangeSize", 0.1f + delayTime);

        if (final)
            Invoke("SetScaleIsBeingChangedToFalse", 0.1f + delayTime + 0.1f);
    }
    void ChangeSize()
    {
        object[] parameter = parameters.Dequeue();

        GameObject toChange = (GameObject)parameter[0];
        Vector3 newScale = (Vector3)parameter[1];

        toChange.transform.localScale = newScale;
    }
    void SetScaleIsBeingChangedToFalse()
    {
        scaleIsBeingChanged = false;
    }
}
