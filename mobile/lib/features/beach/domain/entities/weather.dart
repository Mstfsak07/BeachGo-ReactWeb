class Weather {
  const Weather({
    required this.temperature,
    required this.description,
    required this.windSpeed,
    required this.seaTemperature,
    required this.waveHeight,
  });

  final double? temperature;
  final String description;
  final double? windSpeed;
  final double? seaTemperature;
  final double? waveHeight;
}
