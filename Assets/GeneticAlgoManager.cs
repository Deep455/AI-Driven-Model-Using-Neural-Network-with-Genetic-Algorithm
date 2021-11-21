using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticAlgoManager : MonoBehaviour
{
    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    // void Update()
    // {
        
    // }


    [Header("References")]
    public OurVehicleController ourVehicleController;

    [Header("Controls")]
    public int intial_population = 85;
    [Range(0.0f, 1.0f)]
    public float mutation_rate = 0.055f;

    [Header("Crossover Controls")]
    public int best_selection = 8;
    public int wrost_selection = 3;
    public int number_to_crossover;

    private List<int> genetic_pool = new List<int>();
    private int naturally_selected;
    private NeuralNetwork[] population;


    [Header("Public View")]
    public int current_generation;
    public int current_genome;
    

    private void Start(){
        CreateOurPopulation();
    }

    private void CreateOurPopulation(){
        population = new NeuralNetwork[intial_population];
        FillingPopulationByRandomValues(population, 0);
        ResetingToCurrentGenome();
    }

    private void ResetingToCurrentGenome(){
        ourVehicleController.ResetByNetwork(population[current_genome]);
    }

    private void FillingPopulationByRandomValues(NeuralNetwork[] new_population, int start_index){

        while(start_index < intial_population){
            new_population[start_index] = new NeuralNetwork();
            new_population[start_index].Initialise(ourVehicleController.LAYERS, ourVehicleController.NEURONS);
            start_index++;
        }
    }

    public void Death(float fitness, NeuralNetwork network){

        if (current_genome < population.Length - 1){
            population[current_genome].fitness = fitness;
            current_genome++;
            ResetingToCurrentGenome();
        }

        else{
            RePopulating();
        }
    }

    public void RePopulating(){
        genetic_pool.Clear();
        current_generation++;
        naturally_selected = 0;
        SortingPopulation();

        NeuralNetwork[] new_population = PickingUpBestPopulation();

        CrossoverPopulation(new_population);

        MutatePopulation(new_population);

        FillingPopulationByRandomValues(new_population, naturally_selected);

        population = new_population;

        current_genome = 0;

        ResetingToCurrentGenome();

    }


    private void MutatePopulation(NeuralNetwork[] new_population){

        for(int i=0; i<naturally_selected; i++){

            for(int j=0; j<new_population[i].weights_matrix.Count; j++){

                if(Random.Range(0.0f, 1.0f) < mutation_rate){
                    
                    new_population[i].weights_matrix[j] = MutateMatrix(new_population[i].weights_matrix[j]);

                }
            }
        }
    }

    Matrix<float> MutateMatrix(Matrix<float> matA){

        int random_points = Random.Range(1, (matA.RowCount * matA.ColumnCount) / 7);

        Matrix<float> matB = matA;

        for(int i=0; i<random_points; i++){

            int random_column = Random.Range(0, matB.ColumnCount);
            int random_row = Random.Range(0, matB.RowCount);

            matB[random_row, random_column] = Mathf.Clamp(matB[random_row, random_column] + Random.Range(-1f, 1f), -1f, 1f);

        }

        return matB;

    }

    private void CrossoverPopulation(NeuralNetwork[] new_population){

        for(int i=0; i<number_to_crossover; i+=2){
            
            int A_index = i;
            int B_index = i+1;

            if(genetic_pool.Count >= 1){
                
                for(int x=0; x<100; x++){
                    A_index = genetic_pool[Random.Range(0, genetic_pool.Count)];
                    B_index = genetic_pool[Random.Range(0, genetic_pool.Count)];

                    if(A_index != B_index){
                        break;
                    }

                }
            }

            NeuralNetwork child1 = new NeuralNetwork();
            NeuralNetwork child2 = new NeuralNetwork();

            child1.Initialise(ourVehicleController.LAYERS, ourVehicleController.NEURONS);
            child2.Initialise(ourVehicleController.LAYERS, ourVehicleController.NEURONS);

            child1.fitness = 0;
            child2.fitness = 0;


            for(int wght=0; wght<child1.weights_matrix.Count; wght++){

                if(Random.Range(0.0f, 1.0f) < 0.5f){
                
                    child1.weights_matrix[wght] = population[A_index].weights_matrix[wght];
                    child2.weights_matrix[wght] = population[B_index].weights_matrix[wght];
                
                }
                else{

                    child2.weights_matrix[wght] = population[A_index].weights_matrix[wght];
                    child1.weights_matrix[wght] = population[B_index].weights_matrix[wght];
                
                }
            }


            for(int wght=0; wght<child1.biases.Count; wght++){

                if(Random.Range(0.0f, 1.0f) < 0.5f){
                
                    child1.biases[wght] = population[A_index].biases[wght];
                    child2.biases[wght] = population[B_index].biases[wght];
                
                }
                else{

                    child2.biases[wght] = population[A_index].biases[wght];
                    child1.biases[wght] = population[B_index].biases[wght];
                
                }
            }

            new_population[naturally_selected] = child1;
            naturally_selected++;

            new_population[naturally_selected] = child2;
            naturally_selected++;

        }


    }




    private NeuralNetwork[] PickingUpBestPopulation(){
        
        NeuralNetwork[] new_population = new NeuralNetwork[intial_population];

        for(int i=0; i<best_selection; i++){
            new_population[naturally_selected] = population[i].InitialiseCopy(ourVehicleController.LAYERS, ourVehicleController.NEURONS);
            new_population[naturally_selected].fitness = 0;
            
            naturally_selected++;

            int ff = Mathf.RoundToInt(population[i].fitness * 10);

            for(int j=0; j<ff; j++){

                genetic_pool.Add(i);

            }

        }

        for(int i=0; i<wrost_selection; i++){

            int last = population.Length - 1;
            last-=i;

            
            int ff = Mathf.RoundToInt(population[last].fitness * 10);

            for(int j=0; j<ff; j++){

                genetic_pool.Add(last);

            }

        }

        return new_population;


    }

    public void SortingPopulation(){
        
        // Bubble Sorting
        for(int i=0; i<population.Length; i++){
            
            for(int j=i; j<population.Length;j++){

                if(population[i].fitness < population[j].fitness){
                    NeuralNetwork tmp = population[i];
                    population[i] = population[j];
                    population[j] = tmp;
                }

            }
        }
    }






}
