using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuReturn : MonoBehaviour
{
    public GameObject[] enable;
    public GameObject[] disable;
    public AudioClip returnSound;

    public GameObject transitionOut;
    public CarSelect carSelect;


    private void Awake()
    {
        if (transitionOut != null && carSelect != null && MenuManager.Instance != null)
        {
            MenuManager.Instance.transitionOut = transitionOut;
            MenuManager.Instance.carSelect = carSelect;
        }
    }


    public void OnReturn()
    {
        StartCoroutine(Return());
    }

    private IEnumerator Return()
    {

        for (int i = 0; i < disable.Length; i++)
        {
            if (disable[i].gameObject.GetComponent<Animator>())
                disable[i].gameObject.GetComponent<Animator>().SetTrigger("Out");

            Transform[] child = disable[i].gameObject.transform.GetComponentsInChildren<Transform>();
            for (var x = 0; x < child.Length; x++)
            {
                if (child[x].gameObject.GetComponent<Animator>() && child[x].gameObject != gameObject)
                    child[x].gameObject.GetComponent<Animator>().SetTrigger("Out");
            }
        }

        SoundManager.Instance.PlaySound(returnSound, 0.8f);

        yield return new WaitForSeconds(0.5f);

        MenuManager.Instance.returnTo = enable[0].GetComponent<MenuReturn>();

        MenuManager.Instance.EnableNewCards(enable);

        for (int i = 0; i < disable.Length; i++)
        {
            disable[i].gameObject.SetActive(false);
        }
    }



    public void ReturnButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            MenuManager.Instance.ReturnButtonPressed();
    }

    public void DirectionPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MenuManager.Instance.DirectionPressed();
        }
    }

    public void UpDirectionPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MenuManager.Instance.UpDirectionPressed();
        }
    }

    public void DownDirectionPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MenuManager.Instance.DownDirectionPressed();
        }
    }
}
