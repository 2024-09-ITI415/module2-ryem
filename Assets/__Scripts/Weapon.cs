using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible weapon types.
/// </summary>
public enum WeaponType
{
    none, // The default / no weapon
    blaster, // A simple blaster
    spread, // Two shots simultaneously
    phaser, // [NI] Shots that move in waves
    missile, // [NI] Homing missiles
    laser, // [NI] Damage over time
    shield // Raises shieldLevel
}

/// <summary>
/// The WeaponDefinition class allows you to set the properties
/// of a specific weapon in the Inspector.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public WeaponType type = WeaponType.none;
    public string letter; // Letter to show on the power-up
    public Color color = Color.white; // Color of the collar & power-up
    public GameObject projectilePrefab; // Prefab for projectiles
    public Color projectileColor = Color.white;
    public float damageOnHit = 0; // Amount of damage caused
    public float continuousDamage = 0; // Damage per second (for laser)
    public float delayBetweenShots = 0; // Delay between shots
    public float velocity = 20; // Speed of projectiles
}

/// <summary>
/// The Weapon class handles weapon behavior, using WeaponDefinition to get properties.
/// </summary>
public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")]
    [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime; // Time last shot was fired
    private Renderer collarRend;
    private GameObject laserProjectile; // Holds the active LaserProjectile GameObject
    private Coroutine laserCoroutine;

    private void Start()
    {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();

        // Call SetType() for the default _type of WeaponType.none
        SetType(_type);

        // Dynamically create an anchor for all Projectiles
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        // Find the fireDelegate of the root GameObject
        GameObject rootGO = transform.root.gameObject;
        if (rootGO.GetComponent<Hero>() != null)
        {
            rootGO.GetComponent<Hero>().fireDelegate += Fire;
        }
    }

    public WeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    public void SetType(WeaponType wt)
{
    _type = wt;
    if (type == WeaponType.none)
    {
        this.gameObject.SetActive(false);
        if (laserProjectile != null) Destroy(laserProjectile); // Destroy laser if switching to none
        return;
    }
    else
    {
        this.gameObject.SetActive(true);
    }
    
    def = Main.GetWeaponDefinition(_type);
    collarRend.material.color = def.color;
    lastShotTime = 0; // Allow firing immediately after setting type

    // Destroy the laserProjectile if switching away from laser type
    if (type != WeaponType.laser && laserProjectile != null)
    {
        Destroy(laserProjectile);
    }
}


    public void Fire()
{
    // If this gameObject is inactive, return
    if (!gameObject.activeInHierarchy) return;

    // Non-laser weapons fire normally based on delayBetweenShots
    if (type != WeaponType.laser && Time.time - lastShotTime < def.delayBetweenShots)
    {
        return;
    }

    if (type == WeaponType.laser)
    {
        // Continuous laser handling
        if (laserProjectile == null) // Instantiate laser only if it doesn't exist
        {
            // Instantiate the LaserProjectile prefab
            laserProjectile = Instantiate(def.projectilePrefab);
            laserProjectile.transform.SetParent(PROJECTILE_ANCHOR, true);
        }
        
        // Position the laser at the collar's position, aligning it with the hero
        laserProjectile.transform.position = collar.transform.position;
        laserProjectile.transform.rotation = collar.transform.rotation;

        // Update lastShotTime to handle continuous damage timing if needed
        lastShotTime = Time.time;
    }
    else
    {
        // Handle other non-laser weapon types
        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;
        if (transform.up.y < 0) vel.y = -vel.y;

        switch (type)
        {
            case WeaponType.blaster:
                p = MakeProjectile();
                p.rigid.velocity = vel;
                lastShotTime = Time.time;
                break;

            case WeaponType.spread:
                FireSpread(vel);
                lastShotTime = Time.time;
                break;
        }
    }
}


    private void FireSpread(Vector3 vel)
    {
        Projectile p;
        p = MakeProjectile(false);
        p.rigid.velocity = vel;

        // Right projectiles
        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(15, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(20, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(25, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;


        // Left projectiles
        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(-15, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(-20, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;

        p = MakeProjectile(false);
        p.transform.rotation = Quaternion.AngleAxis(-25, Vector3.back);
        p.rigid.velocity = p.transform.rotation * vel;
    }

    private void ContinuousLaser(Vector3 vel)
{
    Projectile p;
    p = MakeProjectile(false);
    p.rigid.velocity = vel;

    // Right projectiles
    p = MakeProjectile(false);
    p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
    p.rigid.velocity = p.transform.rotation * vel;

    p = MakeProjectile(false);
    p.transform.rotation = Quaternion.AngleAxis(20, Vector3.back);
    p.rigid.velocity = p.transform.rotation * vel;

    // Left projectiles
    p = MakeProjectile(false);
    p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
    p.rigid.velocity = p.transform.rotation * vel;

    p = MakeProjectile(false);
    p.transform.rotation = Quaternion.AngleAxis(-20, Vector3.back);
    p.rigid.velocity = p.transform.rotation * vel;
}

    

    public Projectile MakeProjectile(bool updateLastShotTime = true)
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.parent.gameObject.tag == "Hero")
        {
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        }
        else
        {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        if (updateLastShotTime)
        {
            lastShotTime = Time.time;
        }
        return p;
    }

    private void OnDisable()
    {
        // Stop laser coroutine if weapon is disabled or type changes
        if (laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }
    }
    private void OnTriggerStay(Collider other)
{
    if (type == WeaponType.laser && other.CompareTag("Enemy"))
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Apply continuous damage defined in WeaponDefinition
            //enemy.TakeDamage(def.continuousDamage * Time.deltaTime);
        }
    }
}

}
