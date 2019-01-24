using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TapAndHold : MonoBehaviour
{
    readonly float yToAdd = 0.25f; // amount of y coords to add for each new mash
    bool isHeld;
    bool gameLost;
    GameObject latestMash, currentMash;
    new GameObject camera;

    void Start()
    {
        latestMash = currentMash = GameObject.FindGameObjectWithTag("FirstCylinder");
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

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (isHeld)
            {
                currentMash.transform.localScale += new Vector3(0.01f, 0, 0.01f);

                if (currentMash.transform.localScale.x >= latestMash.transform.localScale.x * 1.1f) // x = z, no need to check z
                {
                    OnLosingTheGame();
                }
            }
            else
            {
                isHeld = true;

                GameObject newMash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                Vector3 latestPosition = latestMash.transform.position;

                newMash.transform.position = new Vector3(latestPosition.x, latestPosition.y + yToAdd, latestPosition.z);
                newMash.transform.localScale = new Vector3(0, yToAdd, 0);

                currentMash = Instantiate(newMash);

                latestPosition = newMash.transform.position;
                camera.transform.position = camera.transform.position + new Vector3(0, yToAdd, 0);
            }
        }
        else
        {
            //  isHeld = false; // ?? can be removed i guess

            if (currentMash != latestMash)
            {
                if ((currentMash.transform.localScale - latestMash.transform.localScale).x >= 0)
                {
                    OnLosingTheGame();
                    return;
                }
                else if ((currentMash.transform.localScale - latestMash.transform.localScale).x >= -0.5f)
                    OnPerfectMove();

                latestMash = currentMash;
            }
        }
    }
    void OnLosingTheGame()
    {
        currentMash.GetComponent<Renderer>().material.color = Color.red;
        camera.transform.position = camera.transform.position + new Vector3(0, 0, -4f);
        gameLost = true;
        Invoke("DestroyCurrentMash", 0.5f);
    }
    void DestroyCurrentMash()
    {
        Destroy(currentMash);
    }
    void OnPerfectMove()
    {
        currentMash.transform.localScale += new Vector3(0.4f, 0, 0.4f);
    }
    IEnumerator MakeBigger(GameObject gameObject, Vector3 value)
    {
        float valueToSubstract = 1.0f;
        Vector3 vectorToSubstract = new Vector3(valueToSubstract, valueToSubstract, valueToSubstract);

        while (value.x > float.Epsilon)
        {
            gameObject.transform.localScale += value - (value - vectorToSubstract);
            value -= vectorToSubstract;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
