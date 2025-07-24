# Azure SQL Database on macOS — Troubleshooting

## macOS Packet Filter (pf)

The macOS built-in firewall uses `pf` (Packet Filter). If Azure SQL connections are timing out or being refused, `pf` may be blocking outbound traffic on port 1433.

```bash
# Turn firewall ON
sudo pfctl -e

# Turn firewall OFF
sudo pfctl -d

# Check if enabled
sudo pfctl -s info

# Show loaded rules
sudo pfctl -sr | cat
```

**Tip**: If `pfctl -s info` shows `Status: Enabled` and you're unable to connect, temporarily disable with `sudo pfctl -d` to confirm whether `pf` is the culprit.
