using BBStats.Data;

public static class Glicko2Calculator
{
	private const double PI2 = Math.PI * Math.PI;
	private const double TAU = 0.5;
	private const double EPSILON = 0.000001;

	public static (Rating, Rating) CalculateRating(Rating playerA, Rating playerB, bool isPlayerAWin)
	{
		// Convert to Glicko-2 scale
		double muA = (playerA.CurrentRating - 1500) / 173.7178;
		double phiA = playerA.RatingDeviation / 173.7178;
		double sigmaA = playerA.Volatility;

		double muB = (playerB.CurrentRating - 1500) / 173.7178;
		double phiB = playerB.RatingDeviation / 173.7178;
		double sigmaB = playerB.Volatility;

		double scoreA = isPlayerAWin ? 1.0 : 0.0;
		double scoreB = 1.0 - scoreA;

		var (newMuA, newPhiA, newSigmaA) =
			UpdatePlayer(muA, phiA, sigmaA, muB, phiB, scoreA);

		var (newMuB, newPhiB, newSigmaB) =
			UpdatePlayer(muB, phiB, sigmaB, muA, phiA, scoreB);

		return (
			new Rating(
				newMuA * 173.7178 + 1500,
				newPhiA * 173.7178,
				newSigmaA
			),
			new Rating(
				newMuB * 173.7178 + 1500,
				newPhiB * 173.7178,
				newSigmaB
			)
		);
	}

	private static (double mu, double phi, double sigma) UpdatePlayer(
		double mu, double phi, double sigma,
		double muOpponent, double phiOpponent, double score)
	{
		// g(phi_j)
		double g = 1.0 / Math.Sqrt(1.0 + 3.0 * phiOpponent * phiOpponent / PI2);

		// E(mu, mu_j, phi_j)
		double E = 1.0 / (1.0 + Math.Exp(-g * (mu - muOpponent)));

		// v
		double v = 1.0 / (g * g * E * (1.0 - E));

		// Δ
		double delta = v * g * (score - E);

		// σ'
		double newSigma = UpdateVolatility(sigma, phi, v, delta);

		// φ*
		double phiStar = Math.Sqrt(phi * phi + newSigma * newSigma);

		// φ'
		double phiPrime = 1.0 / Math.Sqrt(1.0 / (phiStar * phiStar) + 1.0 / v);

		// μ'
		double muPrime = mu + phiPrime * phiPrime * g * (score - E);

		return (muPrime, phiPrime, newSigma);
	}

	private static double UpdateVolatility(double sigma, double phi, double v, double delta)
	{
		double a = Math.Log(sigma * sigma);
		double delta2 = delta * delta;
		double phi2 = phi * phi;

		double A = a;
		double B;

		if (delta2 > phi2 + v)
		{
			B = Math.Log(delta2 - phi2 - v);
		}
		else
		{
			int k = 1;
			while (F(a - k * TAU, delta2, phi2, v, a) < 0)
				k++;
			B = a - k * TAU;
		}

		double fA = F(A, delta2, phi2, v, a);
		double fB = F(B, delta2, phi2, v, a);

		while (Math.Abs(B - A) > EPSILON)
		{
			double C = A + (A - B) * fA / (fB - fA);
			double fC = F(C, delta2, phi2, v, a);

			if (fC * fB <= 0)
			{
				A = B;
				fA = fB;
			}
			else
			{
				fA /= 2.0;
			}

			B = C;
			fB = fC;
		}

		return Math.Exp(A / 2.0);
	}

	private static double F(double x, double delta2, double phi2, double v, double a)
	{
		double ex = Math.Exp(x);
		return
			ex * (delta2 - phi2 - v - ex) /
			(2.0 * Math.Pow(phi2 + v + ex, 2))
			- (x - a) / (TAU * TAU);
	}
}
