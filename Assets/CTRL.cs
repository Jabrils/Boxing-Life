using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class CTRL : MonoBehaviour
{
    public enum Mode { Menu, Fight, KOP1, KOP2, Congrats, Upgrade, Transition, GameOver, WinScreen}
    public Mode mode;
    public GameObject[] boxerObj;
    public Image fadeScreen, upgrade, shopSel, kLeft,kRight;
    public Button dShopping;//, aEnergy, iEnergy;

    public float dmgVisual = 2;
    public int money, level;
    public TextMeshProUGUI txtMoney, txtCountdown, shopDescription, shopCost;
    public RectTransform koBarTrans, energy, energyPrev;
    public bool[] blocking = new bool[2], dodgingL = new bool[2], dodgingR = new bool[2];
    public Image[] shopImg = new Image[6];
    public GameObject[] pow = new GameObject[2];
    public Vector2 showMe;

    Vector2 chgEnergy;
    ShopItem[] shopItem = new ShopItem[6];
    float countdownLerper, chgImprBreathing, chgRecovery, chgStrength, chgStamina;
    Vector3 countdownLerp;
    double[] koBarVar = new double[2];
    Boxer[] boxer = new Boxer[2];
    bool eneDown { get { return mode == Mode.KOP2; } }
    int countdown = 1, shopLocation, subTotal, difficultyExponential = 1;
    int[] pot = new int[2];
    AnimHandler[] next;
    Animator[] anim = new Animator[2];
    ChromaticAberration cA;

    /*
     * HP you can buy to increase chances of not falling
     * Breathing can buy to increase the amount of HP you recover when enemy is KO
     * Recovery can buy to increase the amount of HP you recover when you fall
     * Strength to do heavier hits
     * Stamina to be able to get up easier
     */

    // Start is called before the first frame update
    void Start()
    {
        next = GameObject.Find("All Boxers").GetComponentsInChildren<AnimHandler>();

        fadeScreen.color = Color.clear;

        shopItem[0] = new ShopItem("Add Energy", "Will give you more energy for the next fight.", 5);
        shopItem[1] = new ShopItem("Increase Energy", "This will increase your max energy.", 15);
        shopItem[2] = new ShopItem("Improve Breathing", "When you KO a foe, you will get more hp back with better breathing.", 25);
        shopItem[3] = new ShopItem("Better Recovery", "Will give you back more energy after getting up from being KOed.", 30);
        shopItem[4] = new ShopItem("More Strength", "You will pack a more powerful punch.", 50);
        shopItem[5] = new ShopItem("Longer Stamina", "This will make your getting up game better.", 70);

        //
        for (int i = 0; i < shopImg.Length; i++)
        {
            shopImg[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{shopItem[i].item}\n\n${shopItem[i].cost}";
        }

        // 
        for (int i = 0; i < 2; i++)
        {
            boxer[i] = new Boxer();
            anim[i] = boxerObj[i].GetComponent<Animator>();

            Texture2D tt = new Texture2D(16, 1);

            tt = GetNewBoxerTexture();

            //
            foreach (SkinnedMeshRenderer smr in boxerObj[i].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.material.mainTexture = tt;
            }

        pow[i].SetActive(false);
        }

        boxer[1].RandInit(2);

        //
        foreach (SkinnedMeshRenderer smr in boxerObj[0].GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            smr.material.mainTexture = MenuCTRL.main;
        }

        // 
        for (int i = 0; i < next.Length; i++)
        {
            Texture2D tt = new Texture2D(16, 1);

            tt = GetNewBoxerTexture();

            //
            foreach (SkinnedMeshRenderer smr in next[i].GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                smr.material.mainTexture = tt;
                smr.transform.parent.GetComponent<Animator>().SetTrigger("Idle");
            }
        }

        PostProcessVolume volume = Camera.main.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out cA);

        StartCoroutine(AI());
    }

    IEnumerator DoPow(int who, int color)
    {
        pow[who].GetComponent<SpriteRenderer>().color = color == 0 ? Color.white : Color.green;
        pow[who].SetActive(true);
        yield return new WaitForSeconds(.1f);
        pow[who].SetActive(false);

    }

    void AddToCart(int where, bool add)
    {
        if (where == 0)
        {
            AddEnergy(shopItem[where].cost,add);
        }
        else if (where == 1)
        {
            IncreaseEnergy(shopItem[where].cost, add);
        }
        else if (where == 2)
        {
            ImproveBreathing(shopItem[where].cost, add);
        }
        else if (where == 3)
        {
            Recovery(shopItem[where].cost, add);
        }
        else if (where == 4)
        {
            Strength(shopItem[where].cost, add);
        }
        else if (where == 5)
        {
            Stamina(shopItem[where].cost, add);
        }
        else if (where == 6)
        {
            StartCoroutine(CheckOut());
        }
    }

    void Stamina(int cost, bool add)
    {
        if (add)
        {
            if (subTotal + cost <= money)
            {
                chgStamina++;
                subTotal += cost;
            }
        }
        else
        {
            chgStamina--;
            subTotal -= cost;
        }
    }

    void Strength(int cost, bool add)
    {
        if (add)
        {
            if (subTotal + cost <= money)
            {
                chgStrength++;
                subTotal += cost;
            }
        }
        else
        {
            chgStrength--;
            subTotal -= cost;
        }
    }

    void Recovery(int cost, bool add)
    {
        if (add)
        {
            if (subTotal + cost <= money)
            {
                chgRecovery++;
                subTotal += cost;
            }
        }
        else
        {
            chgRecovery--;
            subTotal -= cost;
        }
    }

    void ImproveBreathing(int cost, bool add)
    {
        if (add)
        {
            if (subTotal + cost <= money)
            {
                chgImprBreathing++;
                subTotal += cost;
            }
        }
        else
        {
            chgImprBreathing--;
            subTotal -= cost;
        }
    }

    IEnumerator CheckOut()
    {
        money -= subTotal;
        boxer[0].ChangeEnergy(chgEnergy.x);
        boxer[0].IncreaseEnergy(chgEnergy.y);
        boxer[0].ImproveBreathing(chgImprBreathing);
        boxer[0].FasterRecovery(chgRecovery);
        boxer[0].GetStronger(chgStrength);
        boxer[0].LongerStamina(chgStamina);

        subTotal = 0;
        chgEnergy = new Vector2(0,0);
        chgImprBreathing = 0;
        chgRecovery = 0;
        chgStrength = 0;
        chgStamina = 0;

        yield return new WaitForSeconds(1);

        mode = Mode.Transition;
    }

    void AddEnergy(int cost, bool add)
    {
        if (add)
        {
            if (boxer[0].energy + chgEnergy.x < boxer[0].maxEnergy + chgEnergy.y && subTotal + cost <= money)
            {
                subTotal += cost;
                chgEnergy.x++;
            }
        }
        else
        {
            subTotal -= cost;
            chgEnergy.x--;
        }
    }

    void IncreaseEnergy(int cost, bool add)
    {
        if (add)
        {
            if (subTotal + cost <= money)
        {
            subTotal += cost;
            chgEnergy.y++;
        }
        }
        else
        {
            subTotal -= cost;
            chgEnergy.y--;
        }
    }

    public static Texture2D SetNewBoxerTexture(Color[] c)
    {
        Texture2D tt;
                c[0] = (Color.white + c[0]) / 2;
        c[15] = Color.white;

        // 
        tt = new Texture2D(16, 1);
        tt.SetPixels(c);
        tt.Apply();
        return tt;
    }

    public static Texture2D GetNewBoxerTexture()
    {
        Texture2D tt;
        Color[] c = new Color[16];

        // 
        for (int i = 0; i < c.Length; i++)
        {
            c[i] = new Color(Random.value, Random.value, Random.value);

            if (i == 0)
            {
                c[i] = (Color.white + c[i]) / 2;
            }
        }

        c[15] = Color.white;

        // 
        tt = new Texture2D(16, 1);
        tt.SetPixels(c);
        tt.Apply();
        return tt;
    }

    // 
    IEnumerator AI()
    {
        yield return new WaitForSeconds(Random.value * boxer[1].aiButtonRate / (eneDown ? 1 : 1));

        float dice1 = Random.value;

        float dice2 = Random.value;

        char side = dice2 >= .5f ? 'R' : 'L';

        // 
        if (boxer[1].hasEnergy && mode == Mode.Fight)
        {
            if (dice1 > .5f)
            {
                Punch(1, side, "High");
            }
            else if (dice1 < .25f)
            {
                anim[1].SetTrigger("Block");
            }
            else
            {
                float dice3 = Random.value;

                if (dice3 > .5f)
                {
                    anim[1].SetTrigger("DodgeR");
                }
                else
                {
                    anim[1].SetTrigger("DodgeL");
                }
            }
        }
        else if (mode == Mode.KOP2)
        {
            koBarVar[1] += boxer[1].stamina;
        }

        StartCoroutine(AI());
    }

    // Update is called once per frame
    void Update()
    {
        Updaters();

        // 
        if (mode == Mode.Fight)
        {
            if (boxer[0].hasEnergy && !animState(0,"KO"))
            {
                Inputs();
            }
        }
        else if (mode == Mode.KOP1)
        {
            GettingUpSequence();
        }
        else if (mode == Mode.KOP2)
        {
            VarInteractor(1);

            anim[1].Play("Get Up", 0, (float)koBarVar[1]);
        }
    }

    void GettingUpSequence()
    {
        VarInteractor(0);

        bool right = false;
        float delta = 1;

        if (mode == Mode.KOP1)
        {
            if (pot[1] != 0)
            {
                delta = (pot[0] / pot[1]);
            }
            else
            {
                delta = 1;
            }
        }
        else if (mode == Mode.KOP1)
        {
            if (pot[0] != 0)
            {
                delta = (pot[1] / pot[0]);
            }
            else
            {
                delta = 1;
            }
        }

        if (delta ==0)
        {
            delta = 1;
        }
        // 
        if (!right)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                koBarVar[0] += boxer[0].stamina * delta;
                right = true;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                koBarVar[0] += boxer[0].stamina * delta;
                right = true;
            }
        }

        anim[0].Play("Get Up", 0, (float)koBarVar[0]);
    }

    void VarInteractor(int w)
    {
        //float delta = mode == Mode.KOP1 ? (pot[1] / pot[0]) : mode == Mode.KOP2 ? (pot[0] / pot[1]) : 1;
        //print(delta);
        koBarVar[w] -= .001f; // - ((hp[w].x - (hp[w].y / 2) / hp[w].y - (hp[w].y/2)) * (.001f));

        koBarVar[w] = Mathf.Clamp01((float)koBarVar[w]);

        // 
        if (koBarVar[w] >= 1)
        {
            mode = Mode.Fight;
            boxer[w].Recover();
            anim[w].SetTrigger("Reset");
        }
    }

    void Inputs()
    {
        // Punch Right or Uppercut
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (animState(0, "DodgeL"))
            {
                anim[0].SetTrigger("UpperL");
            }
            else if (!animState(0, "PunchL_High"))
            {
                Punch(0, 'L', "High");
            }
        }

        // Punch Left or Uppercut
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (animState(0, "DodgeR"))
            {
                anim[0].SetTrigger("UpperR");
            }
            else if (!animState(0, "PunchR_High"))
            {
                Punch(0, 'R', "High");
            }
        }

        // Block
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            anim[0].SetTrigger("Block");
        }

        // Dodge Right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            anim[0].SetTrigger("DodgeR");
        }

        // Dodge Left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            anim[0].SetTrigger("DodgeL");
        }
    }

    void Updaters()
    {
        // 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // 
        if (Input.GetKeyDown(KeyCode.U))
        {
            boxer[0].ChangeEnergy(-100);
        }

        showMe.x = boxer[0].energy;
        showMe.y = boxer[0].maxEnergy;

        string sign = Mathf.Sign(pot[0]) == 1 ? "+" : "-";
        txtMoney.text = $"${money} {sign} ${Mathf.Abs(pot[0])}";
        txtCountdown.text = $"{countdown}";

        countdownLerper += Time.deltaTime*3;
        txtCountdown.rectTransform.localScale = Vector3.Lerp(countdownLerp, Vector3.one, countdownLerper);

        cA.intensity.value = (boxer[0].maxEnergy - boxer[0].energy) * dmgVisual / boxer[0].maxEnergy;

        koBarTrans.transform.localScale = new Vector3((float)koBarVar[0], 1, 1);

        kLeft.transform.localScale = Vector3.one * (Mathf.Sin(Time.time*25)+1) * .5f;
        kRight.transform.localScale = Vector3.one * (Mathf.Sin((Time.time + Mathf.PI)*25 )+1) * .5f;

        koBarTrans.parent.gameObject.SetActive(mode == Mode.KOP1);
        txtCountdown.gameObject.SetActive(mode == Mode.KOP1 || mode == Mode.KOP2);
        upgrade.gameObject.SetActive(mode == Mode.Upgrade);

        // 
        if (mode == Mode.Upgrade)
        {
            energy.transform.localScale = new Vector3(Mathf.Clamp01((boxer[0].energy) / (boxer[0].maxEnergy + chgEnergy.y)), 1, 1);
            energyPrev.transform.localScale = new Vector3(Mathf.Clamp01((boxer[0].energy + chgEnergy.x) / (boxer[0].maxEnergy + chgEnergy.y)), 1, 1);
        shopSel.color = new Color(shopSel.color.r, shopSel.color.g, shopSel.color.b, ((Mathf.Sin(Time.time*3)+1)/2) * .66f);

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                shopLocation += 1;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                shopLocation -= 1;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                shopLocation -= 3;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                shopLocation += 3;
            }

            shopLocation = Mathf.Clamp(shopLocation, 0, 6);

            shopCost.text = $"${money - subTotal}";

            // 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddToCart(shopLocation, true);
            }

            // 
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                AddToCart(shopLocation, false);
            }

            shopSel.transform.localPosition = new Vector3(shopLocation == 0 || shopLocation == 3 ? -275 : shopLocation == 1 || shopLocation == 4 ? 0 : 275, shopLocation < 3 ? 116 : -106, 0);
            shopSel.transform.localScale = (shopLocation == 6) ? new Vector3(2.63f, .39f, 1) : Vector3.one;

            //
            if (shopLocation == 6)
            {
                shopSel.transform.localPosition = Vector3.down * 411;
                shopDescription.text = $"Press space to checkout!";
            }
            else
            {
                shopDescription.text = shopItem[shopLocation].description;
            }
        }

        float screenFadeSpeed = .1f;

        // 
        if (mode == Mode.Fight)
        {
            if (fadeScreen.color.a > 0)
            {
                fadeScreen.color -= Color.black * screenFadeSpeed;
            }

            for (int i = 0; i < 2; i++)
            {
                if (!boxer[i].hasEnergy)
                {
                    anim[i].SetTrigger("KO");
                }
            }
        }

        // 
        if (mode == Mode.Transition)
        {
            fadeScreen.color += Color.black * screenFadeSpeed;

            // 
            if (fadeScreen.color == Color.black)
            {
                Vector3 pos = boxerObj[1].transform.position;
                Vector3 rot = boxerObj[1].transform.eulerAngles;

                Destroy(boxerObj[1]);

                int theNext = next.Length - (level + 1);

                next[theNext].gameObject.transform.position = pos;
                next[theNext].gameObject.transform.eulerAngles = rot;
                boxerObj[1] = next[theNext].gameObject;
                anim[1] = boxerObj[1].GetComponent<Animator>();
                level++;
                boxer[1].RandInit(level * difficultyExponential);
                anim[0].SetTrigger("Reset");
                anim[1].SetTrigger("Reset");

                mode = Mode.Fight;
            }
        }

        //screenFadeSpeed = Mathf.Clamp01(screenFadeSpeed);

        // 
        for (int i = 0; i < 2; i++)
        {
            if (animState(i, "Idle"))
            {
                ResetVars(i);
            }

            if (mode == Mode.KOP1 || mode == Mode.KOP2)
            {
                ResetVars(i);
            }
        }
    }

    void ResetVars(int who)
    {
        blocking[who] = false;
        dodgingL[who] = false;
        dodgingR[who] = false;
        anim[who].ResetTrigger("KO");
    }

    public void Hit(int type, int player)
    {
        int focus = player == 0 ? 1 : 0;

        // 
        if (blocking[focus])
        {
            StartCoroutine(DoPow(focus, 1));
            pot[focus] += type == 0 ? Mathf.RoundToInt(boxer[player].strength) : (int)boxer[player].strength * 10;
            boxer[focus].ChangeEnergy(type == 0 ? -boxer[player].strength / 10 : -boxer[player].strength);
        }
        else if (!blocking[focus])
        {
            if (dodgingR[focus] && animState(player, "UpperR"))
            {
                anim[player].SetTrigger("Tired");
            }
            else if (dodgingL[focus] && animState(player, "UpperL"))
            {
                anim[player].SetTrigger("Tired");
            }
            else if (boxer[focus].hasEnergy)
            {
                StartCoroutine(DoPow(focus, 0));
                boxer[focus].ChangeEnergy(type == 0 ? -boxer[player].strength : -boxer[player].strength * 3);

                pot[focus] -= type == 0 ? Mathf.RoundToInt(boxer[player].strength / 2) : Mathf.RoundToInt(boxer[player].strength * 2);
                pot[player] += type == 0 ? (int)boxer[player].strength * 2 : (int)(boxer[player].strength * 10);

                // 
                if (animState(focus, "UpperL") || animState(focus, "UpperR"))
                {
                    anim[focus].SetTrigger("Reset");
                }
            }
        }

        // 
        if (boxer[focus].energy <= 0 && mode == Mode.Fight)
        {
            anim[focus].SetTrigger("KO");
        }
    }

    public void SetBlock(int who, bool on)
    {
        blocking[who] = on;
    }

    public void SetDodge(int who, bool on, char side)
    {
        if (side == 'R')
        {
            dodgingR[who] = on;
        }
        else if (side == 'L')
        {
            dodgingL[who] = on;
        }
    }

    bool animState(int who, string n)
    {
        return anim[who].GetCurrentAnimatorStateInfo(0).IsName(n);
    }

    void Punch(int who, char dir, string height)
    {
                ResetVars(who);
        anim[who].SetTrigger($"Punch{dir}_{height}");
    }

    public void SomeoneKO(int who)
    {
        koBarVar[who] = 0;
        int focus = who == 0 ? 1 : 0;

        boxer[focus].Breath();

        if (who == 0)
        {
            mode = Mode.KOP1;
        }
        else
        {
            mode = Mode.KOP2;
        }

        countdown = 10;
        StartCoroutine(StartCountdown());
    }

    IEnumerator NextFight()
    {
        yield return new WaitForSeconds(3);

        shopLocation = 0;
        mode = Mode.Upgrade;
    }

    IEnumerator StartCountdown()
    {
        if (mode == Mode.KOP1 || mode == Mode.KOP2)
        {
            yield return new WaitForSeconds(1);

            countdownLerper = 0;
            countdownLerp = Vector3.one * .5f;
            countdown--;

            if (countdown == 0)
            {
                if (mode == Mode.KOP1)
                {
                    mode = Mode.GameOver;
                    Win(1);
                }
                else if (mode == Mode.KOP2)
                {
                    mode = Mode.Congrats;
                    money += pot[0];
                    pot[0] = 0;

                    Win(0);

                    StartCoroutine(NextFight());
                }
            }
            else
            {
                StartCoroutine(StartCountdown());
            }
        }
    }

    private void Win(int who)
    {
        int focus = who == 0 ? 1 : 0;

        anim[who].SetTrigger("Win");
        anim[focus].SetTrigger("TKO");
    }
}

class Boxer
{
    Vector2 _energy;
    float _koBar, _recovery, _breathing, _strength, _stamina, _aiButtonRate;

    public float energy { get { return _energy.x; } }
    public float maxEnergy { get { return _energy.y; } }
    public float energyPercent { get { return _energy.x / _energy.y; } }
    public bool hasEnergy { get { return energy > 0; } }
    public bool fullEnergy { get { return _energy.x >= _energy.y; } }
    public float strength { get { return _strength; } }
    public float stamina { get { return _stamina; } }
    public float aiButtonRate { get { return _aiButtonRate; } }

    public Boxer()
    {
        RestoreEnergy();

        _energy = new Vector2(10, 10);

        // Energy Recovered after a successful get up
        _recovery = 1;

        // Base damage you will deal
        _strength = 1;

        _stamina = .025f;
    }

    public void PrintStats()
    {
        MonoBehaviour.print($"Ener: {maxEnergy} / Rate: {_aiButtonRate} / Rec: {_recovery} / Breath: {_breathing} / Str: {_strength} / Stam: {_stamina}");
    }

    public void Init(float hp, float recovery, float breathing)
    {
        RestoreEnergy();
    }

    public void RandInit(int pts)
    {
        RestoreEnergy();

        _aiButtonRate = 10f;

        for (int i = 0; i < pts; i++)
        {
            int dice = Random.Range(0, 5);

            if (dice == 0)
            {
                IncreaseEnergy(1);
            }
            else if (dice == 1)
            {
                FasterRecovery(1);
            }
            else if (dice == 2)
            {
                ImproveBreathing(1);
            }
            else if (dice == 3)
            {
                GetStronger(1);
            }
            else if (dice == 4)
            {
                LongerStamina(1);
            }
            else if (dice == 5)
            {
                DecreaseAIRate(1);
            }
        }

        PrintStats();
    }

    void DecreaseAIRate(float amt)
    {
        _aiButtonRate *= (Mathf.Pow(.9f,amt));
    }

    public void RestoreEnergy()
    {
        _energy.x = _energy.y;
    }

    public void ChangeEnergy(float amt)
    {
        _energy.x += amt;

        _energy.x = Mathf.Clamp(_energy.x, 0, _energy.y);
    }

    public void IncreaseEnergy(float amt)
    {
        _energy.y += amt;
    }

    public void Recover()
    {
        ChangeEnergy(_recovery);
    }

    public void Breath()
    {
        ChangeEnergy(_breathing);
    }

    public void ImproveBreathing(float amt)
    {
        _breathing += amt * .33f;
    }

    public void FasterRecovery(float amt)
    {
        _recovery += amt * .5f;
    }

    public void GetStronger(float amt)
    {
        _strength += amt * .33f;
    }

    public void LongerStamina(float amt)
    {
        _stamina += amt * .0001f;
    }
}

class ShopItem
{

    string _item, _description;
    int _cost;

    public string item { get { return _item; } }
    public string description { get { return _description; } }
    public int cost { get { return _cost; } }

    public ShopItem(string item, string description, int cost)
    {
        _item = item;
        _description = description;
        _cost = cost;
    }
}