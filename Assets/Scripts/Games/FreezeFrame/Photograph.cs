using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HeavenStudio.Common;
using HeavenStudio.Games.Scripts_WizardsWaltz;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_FreezeFrame
{
    public class Photograph : MonoBehaviour
    {
        //[SerializeField] GameObject Cameos;
        [SerializeField] Animator _Animator;

        // Start is called before the first frame update
        void Start()
        {
            HideAll();
            //gameObject.SetActive(false);
        }
        public void ShowPhoto(FreezeFrame.PhotoArgs args)
        {
            SetPhoto(args);
            _Animator.DoScaledAnimationAsync("Show", timeScale: 0.5f, animLayer: 2);
        }
        public void HideAll()
        {
            _Animator.DoScaledAnimationAsync("NoCar", timeScale: 0.5f, animLayer: 0);
            _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
            _Animator.DoScaledAnimationAsync("Hide", timeScale: 0.5f, animLayer: 2);
        }
        public void SetPhoto(FreezeFrame.PhotoArgs args)
        {
            HideAll();

            // complete miss, empty pic
            if (args.State <= -2)
            {
                _Animator.DoScaledAnimationAsync("NoCar", timeScale: 0.5f, animLayer: 0);
                _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                return;
            }

            switch (args.Car)
            {
                case FreezeFrame.CarType.SlowCar:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("SlowCar_Late", timeScale: 0.5f, animLayer: 0);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("SlowCar_Early", timeScale: 0.5f, animLayer: 0);
                    else
                        _Animator.DoScaledAnimationAsync("SlowCar_Perfect", timeScale: 0.5f, animLayer: 0);
                    break;
                case FreezeFrame.CarType.FastCar:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("FastCar_Late", timeScale: 0.5f, animLayer: 0);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("FastCar_Early", timeScale: 0.5f, animLayer: 0);
                    else
                        _Animator.DoScaledAnimationAsync("FastCar_Perfect", timeScale: 0.5f, animLayer: 0);
                    break;
            }

            switch(args.PhotoType)
            {
                case FreezeFrame.PhotoType.Default:
                    _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;
                
                case FreezeFrame.PhotoType.Ninja:
                    if (args.State == 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Ninja", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.Ghost:
                    if (args.State == 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Ghost", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.Rats:
                    if (args.State == 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Rats", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;
                
                case FreezeFrame.PhotoType.PeaceSign:
                    if (args.State == 0)
                    {
                        if (args.Car == FreezeFrame.CarType.SlowCar)
                            _Animator.DoScaledAnimationAsync("Cameo_PeaceSlow", timeScale: 0.5f, animLayer: 1);
                        else
                            _Animator.DoScaledAnimationAsync("Cameo_PeaceFast", timeScale: 0.5f, animLayer: 1);
                    }
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;

                // i don't like using so many dang cases here with basically the same thing in each but it breaks with the other thing i tried
                case FreezeFrame.PhotoType.GirlfriendRight:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Right_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Right_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Right_Perfect", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.GirlfriendLeft:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Left_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Left_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Girlfriend_Left_Perfect", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.Dude1Right:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Right_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Right_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Right_Perfect", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.Dude1Left:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Left_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Left_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Dude1_Left_Perfect", timeScale: 0.5f, animLayer: 1);
                    return;
                case FreezeFrame.PhotoType.Dude2Right:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Right_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Right_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Right_Perfect", timeScale: 0.5f, animLayer: 1);
                    break;
                case FreezeFrame.PhotoType.Dude2Left:
                    if (args.State > 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Left_Late", timeScale: 0.5f, animLayer: 1);
                    else if (args.State < 0)
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Left_Early", timeScale: 0.5f, animLayer: 1);
                    else
                        _Animator.DoScaledAnimationAsync("Cameo_Dude2_Left_Perfect", timeScale: 0.5f, animLayer: 1);
                    return;
                
                default: // should not ever happen but it could
                    _Animator.DoScaledAnimationAsync("Cameo_None", timeScale: 0.5f, animLayer: 1);
                    return;
            }
        }
    }
}