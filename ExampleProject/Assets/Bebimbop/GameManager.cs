﻿using System.Linq;
using Bebimbop.Utilities;
using UnityEngine;
using Bebimbop.Utilities.StateMachine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

namespace Bebimbop.Example
{
    public class GameManager : MonoBehaviour
    {
        public StateMachine<State> MainStateMachine;
        private void Awake()
        {
            //initialize statemachine in Awake() to avoid Null error in other scripts
            MainStateMachine = StateMachine<State>.Initialize(this);
        }

        public InputField DurationInputField;
        public Dropdown TransitionDropdown;
        public GameObject[] Descriptions;
        private void Start()
        {
            //bind dropdown to setmode method
            TransitionDropdown
                .onValueChanged
                .AsObservable()
                .Subscribe(i =>MainStateMachine.SetMode((StateTransition)i)).AddTo(gameObject);
            
            //bind inputfeild to setduration method
            DurationInputField
                .OnDeselectAsObservable()
                .Subscribe(d =>
                {
                    var val = DurationInputField.text;
                    float duration = 0.5f;
                    float.TryParse(val, out duration);
                    MainStateMachine.SetDuration(duration, duration);
                }).AddTo(gameObject);
            
            Descriptions.ToList().ForEach(x => x.SetActive(false));
            
            //you can get notified state changed event
            MainStateMachine.Changed += o => Debug.Log("State Changed : " + o.ToString());
            MainStateMachine.ChangeState(State.RED);
        }
        
        //test key bindings
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Q))
            {
               MainStateMachine.ChangeState(State.RED,StateTransition.Overwrite);
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                MainStateMachine.ChangeState(State.BLUE,StateTransition.Overwrite);
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                MainStateMachine.ChangeState(State.GREEN,StateTransition.Overwrite);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                MainStateMachine.CancelTransition();
            }
        }

        public CanvasGroup UiPanel;
        public RectTransform EnterProgress,ExitProgress;
        
        /// method name format should be STATE_Enter, it gets called only if this method name present in teh script
        private void RED_Enter(float t)
        {
            if (t == 0)
            {
                Debug.Log("Entering RED Time : " + Time.time);
                ExitProgress.sizeDelta = new Vector2(0,30);    
            }
            UiPanel.FadingIn(t);
            EnterProgress.sizeDelta = new Vector2(t.FromTo(0,1,0,1920),30);
            
            if (t == 1) EnterProgress.sizeDelta = new Vector2(0,30);
        }
        private void RED_EnterCancel()
        {
            Debug.Log("RED Enter cancled");
            UiReset();
        }
        
        
        private void RED_Exit(float t)
        {
            if (t == 0)
            {
                Debug.Log("Exiting RED Time : " + Time.time);
                EnterProgress.sizeDelta = new Vector2(0,30);
            }
            UiPanel.FadingOut(t);
            ExitProgress.sizeDelta = new Vector2(t.FromTo(0,1,0,1920),30);
            if(t == 1)  ExitProgress.sizeDelta = new Vector2(0,30);
        }
        private void RED_ExitCancel()
        {
            Debug.Log("RED Exit cancled");
            UiPanel.EnableCanvasGroup(true);
        }
        
        
        private void RED_Finally()
        {
            Debug.Log("RED Finally called");
            UiReset();
        }
        private void UiReset()
        {
            EnterProgress.sizeDelta = new Vector2(0,30);
            ExitProgress.sizeDelta = new Vector2(0,30);
            UiPanel.DisableCanvasGroup(true);
        }

        public void ChangeState(int num)
        {
            MainStateMachine.ChangeState((State)num);
        }

        public void Cancel()
        {
            MainStateMachine.CancelTransition();
        }

        public enum State
        {
            RED = 0,BLUE, GREEN
        }

#region Singletone behaviour
  private static GameManager instance = null;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.FindObjectOfType<GameManager>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = typeof(GameManager).Name;
                        instance = go.AddComponent<GameManager>();
                    }
                }

                return instance;
            }
        }

        public static bool DoesExist()
        {
            return instance != null;
        }

        // Makes this object a persistent singleton unless the singleton already exists in which case
        // the this object is destroyed
        protected void DontDestroy()
        {
            if (this == Instance)
            {
                // needs to be a root object before calling DontDestroyOnLoad
                Instance.transform.parent = null;
                MonoBehaviour.DontDestroyOnLoad(Instance.gameObject);
            }
            else
            {
                MonoBehaviour.Destroy(this);
            }
        }
#endregion
    }
}

