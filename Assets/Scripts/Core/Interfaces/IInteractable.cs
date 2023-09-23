using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void OnClick();
    public void OnOver();
    public void OnEnter();
    public void OnExit();
}
