using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;
using System;
using Random = UnityEngine.Random;

public class NeuralNetwork : MonoBehaviour
{
    // Start is called before the first frame update
    /* void Start()
    {
        
    } */

    // Update is called once per frame
    /* void Update()
    {
        
    } */

    public Matrix<float> input_layer_matrix = Matrix<float>.Build.Dense(1, 3);      // Input Layer Matrix

    public List<Matrix<float>> hidden_layers_matrices = new List<Matrix<float>>();    // List of Hidden Layers Matrix

    public Matrix<float> output_layer_matrix = Matrix<float>.Build.Dense(1, 2);     // Output Layer Matrix

    public List<Matrix<float>> weights_matrix = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    public void Initialise (int hidden_layers_count, int hidden_neurons_count){

        input_layer_matrix.Clear();
        hidden_layers_matrices.Clear();
        output_layer_matrix.Clear();
        weights_matrix.Clear();
        biases.Clear();

        for (int i=0; i<hidden_layers_count+1; i++){
            Matrix<float> f = Matrix<float>.Build.Dense(1, hidden_neurons_count);
            hidden_layers_matrices.Add(f);
            biases.Add(Random.Range(-1f, 1f));

            if(i==0){
                Matrix<float> hidden_layer_1_input = Matrix<float>.Build.Dense(3, hidden_neurons_count);
                weights_matrix.Add(hidden_layer_1_input);
            }

            Matrix<float> hidden_to_hidden = Matrix<float>.Build.Dense(hidden_neurons_count, hidden_neurons_count);
            weights_matrix.Add(hidden_to_hidden);
        }

        Matrix<float> output_weight = Matrix<float>.Build.Dense(hidden_neurons_count, 2);
        weights_matrix.Add(output_weight);
        biases.Add(Random.Range(-1f, 1f));
        Weight_randomise();
        
    }

    public NeuralNetwork InitialiseCopy(int hidden_layers_count, int hidden_neurons_count){
        NeuralNetwork nn =new NeuralNetwork();

        List<Matrix<float>> new_weights = new List<Matrix<float>>();

        for(int i=0; i < this.weights_matrix.Count; i++){

            Matrix<float> current_weight = Matrix<float>.Build.Dense(weights_matrix[i].RowCount, weights_matrix[i].ColumnCount);

            for(int x=0; x<current_weight.RowCount; x++){

                for(int y=0; y<current_weight.ColumnCount; y++){
                    current_weight[x,y] = weights_matrix[i][x,y];
                }
            }
            new_weights.Add(current_weight);
        }

        List<float> new_biases = new List<float>();

        new_biases.AddRange(biases);

        nn.weights_matrix = new_weights;
        nn.biases = new_biases;

        nn.InitialisingHidden(hidden_layers_count, hidden_neurons_count);

        return nn;

    }

    public void InitialisingHidden(int hidden_layers_count, int hidden_neurons_count){

        input_layer_matrix.Clear();
        hidden_layers_matrices.Clear();
        output_layer_matrix.Clear();

        for(int i=0; i<hidden_layers_count+1; i++){

            Matrix<float> new_hidden_layer = Matrix<float>.Build.Dense(1, hidden_neurons_count);

            hidden_layers_matrices.Add(new_hidden_layer);
        }

    }

    public void Weight_randomise(){
        
        for(int i=0; i < weights_matrix.Count; i++){
            for(int x=0; x < weights_matrix[i].RowCount; x++){
                for(int y=0; y < weights_matrix[i].ColumnCount; y++){
                    weights_matrix[i][x,y] = Random.Range(-1f, 1f);
                }
            }
        }
    }

    public (float, float) Network_run(float a, float b, float c){
        input_layer_matrix[0,0] = a;
        input_layer_matrix[0,1] = b;
        input_layer_matrix[0,2] = c;

        input_layer_matrix = input_layer_matrix.PointwiseTanh();

        hidden_layers_matrices[0] = ((input_layer_matrix * weights_matrix[0]) + biases[0]).PointwiseTanh();

        for(int i=1; i<hidden_layers_matrices.Count; i++){
            hidden_layers_matrices[i] = ((hidden_layers_matrices[i-1]*weights_matrix[i]) + biases[i]).PointwiseTanh();
        }

        output_layer_matrix = ((hidden_layers_matrices[hidden_layers_matrices.Count-1]*weights_matrix[weights_matrix.Count-1]) + biases[biases.Count-1]).PointwiseTanh();

        return (Sigmoid(output_layer_matrix[0,0]), (float)Math.Tanh(output_layer_matrix[0,1]));     // return is (acceleration, steering)
    }

    private float Sigmoid(float temp){
        return (1/(1+Mathf.Exp(-temp)));
    }


}
