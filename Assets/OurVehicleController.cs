using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]

public class OurVehicleController : MonoBehaviour
{
    // Start is called before the first frame update
    /* void Start()
    {
        
    } */

    // Update is called once per frame
    /* void Update()
    {
        
    } */


    private Vector3 start_pos, start_rotation;
    private NeuralNetwork neural_network;

    [Range(-1f, 1f)]

    public float acceleration, turning_value, total_time_since_start=0f;

    [Header("Fitness")]

    // For giving weight coefficient
    public float overall_fitness, average_speed_multiplier=0.2f, distance_multiplier=1.4f, sensor_multiplier = 0.1f;

    [Header("Neural Network Options")]
    public int LAYERS = 1, NEURONS = 10;

    private Vector3 last_pos;
    private float average_speed, total_distance_travelled;
    
    private float sensor1, sensor2, sensor3;

    // First program that will run
    private void Awake(){
        start_pos = transform.position;
        start_rotation = transform.eulerAngles;

        neural_network = GetComponent<NeuralNetwork>();

    }

    public void ResetByNetwork(NeuralNetwork net){
        neural_network = net;
        ResetUp();
    }

    public void ResetUp(){

        total_time_since_start=0f;
        total_distance_travelled = 0f;
        average_speed =0f;
        last_pos =start_pos;
        overall_fitness = 0f;
        transform.position = start_pos;
        transform.eulerAngles = start_rotation;
    }
    
    private void OnCollisionEnter(Collision collision){
        Death();

    }

    private void FixedUpdate(){
        InptSensors();
        last_pos = transform.position;

        // Neural Network down below here

        (acceleration, turning_value) = neural_network.Network_run(sensor1, sensor2, sensor3);

        MoveCar(acceleration, turning_value);
        
        total_time_since_start += Time.deltaTime;

        FitnessCalculator();

        // acceleration = 0;
        // turning_value = 0;
    }

    private void Death(){
        GameObject.FindObjectOfType<GeneticAlgoManager>().Death(overall_fitness, neural_network);
    }

    private void FitnessCalculator(){
        total_distance_travelled += Vector3.Distance(transform.position, last_pos);

        average_speed = total_distance_travelled / total_time_since_start;

        overall_fitness = (total_distance_travelled*distance_multiplier) + (average_speed*average_speed_multiplier) + (((sensor1+sensor2+sensor3)/3)*sensor_multiplier);

        if ( total_time_since_start > 20 && overall_fitness < 40){
            Death();
        }

        if (overall_fitness >= 1000){

            // Could save the result to a JSON file
            Death();
        }

    }

    private void InptSensors(){

        Vector3 a = (transform.forward + transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward - transform.right);

        Ray rr = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(rr, out hit)){
            sensor1 = hit.distance/20;
            Debug.DrawLine(rr.origin, hit.point, Color.red);
        }

        rr.direction = b;
        if (Physics.Raycast(rr, out hit)){
            sensor2 = hit.distance/20;
            Debug.DrawLine(rr.origin, hit.point, Color.red);
        }

        rr.direction = c;
        if (Physics.Raycast(rr, out hit)){
            sensor3 = hit.distance/20;
            Debug.DrawLine(rr.origin, hit.point, Color.red);

        }



    }

    private Vector3 inpt;
    public void MoveCar (float v, float h){
        inpt = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        inpt = transform.TransformDirection(inpt);

        transform.position += inpt;

        transform.eulerAngles += new Vector3(0, Mathf.Lerp(0, h*90,0.02f), 0);

         

    }






}