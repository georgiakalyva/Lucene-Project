using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneProject.Extensions
{
    public static class IntegerArrayExtensions
    {
        /// <summary>
        /// Υπολογίζει το εσωτερικό γινόμενο δύο διανυσμάτων.
        /// </summary>
        /// <param name="vector1">Το πρώτο διάνυσμα.</param>
        /// <param name="vector2">Το δεύτερο διάνυσμα.</param>
        /// <returns>Το αποτέλεσμα της πράξης του εσωτερικού γινομένου των διανυσμάτων.</returns>
        public static int DotProduct(this int[] vector1, int[] vector2)
        {
            if (vector1 == null)
            {
                throw new ArgumentNullException("vector1");
            }

            if (vector1 == null)
            {
                throw new ArgumentNullException("vector2");
            }

            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("The arrays must have the same size.");
            }

            int sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                sum += vector1[i] * vector2[i];
            }
            return sum;
        }

        /// <summary>
        /// Υπολογίζει τα ευκλείδιο μέτρο ενός διανύσματος.
        /// </summary>
        /// <param name="source">Το διάνυσμα.</param>
        /// <returns>Την τιμή του ευκλείδίου μέτρου του διανύσματος.</returns>
        public static double VectorLength(this int[] source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            double sum = 0.0;
            for (int i = 0; i < source.Length; i++)
            {
                sum += source[i] * source[i];
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Υπολογισμός της συνημιτονοειδούς ομοιότητας δύο διανυσμάτων.
        /// </summary>
        /// <param name="vector1">Το πρώτο διάνυσμα.</param>
        /// <param name="vector2">Το δεύτερο διάνυσμα.</param>
        /// <returns>Την τιμή της συνημιτονοειδούς ομοιότητας δύο διανυσμάτων.</returns>
        public static double CosineSimilarity(this int[] vector1, int[] vector2)
        {
            if (vector1 == null)
            {
                throw new ArgumentNullException("vector1");
            }

            if (vector1 == null)
            {
                throw new ArgumentNullException("vector2");
            }

            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("The arrays must have the same size.");
            }

            double denom = (VectorLength(vector1) * VectorLength(vector2));

            if (denom == 0)
            {
                return 0;
            }
            else
            {
                return (DotProduct(vector1, vector2) / denom);
            }
        }
    }
}
