export function seatColor(percent: number): "error" | "warning" | "primary" {
  if (percent > 85) return "error";
  if (percent > 60) return "warning";
  return "primary";
}
