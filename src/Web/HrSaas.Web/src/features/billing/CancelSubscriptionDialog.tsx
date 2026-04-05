import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";

interface CancelSubscriptionDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (reason: string) => void;
  loading: boolean;
}

export function CancelSubscriptionDialog({
  open,
  onClose,
  onConfirm,
  loading,
}: Readonly<CancelSubscriptionDialogProps>) {
  const [reason, setReason] = useState("Cost optimization");

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <DialogTitle>Cancel Subscription</DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" mb={2}>
          Please tell us why you are cancelling. This helps us improve.
        </Typography>
        <TextField
          fullWidth
          multiline
          minRows={2}
          label="Reason"
          value={reason}
          onChange={(e) => setReason(e.target.value)}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Keep Subscription</Button>
        <Button
          variant="contained"
          color="error"
          onClick={() => onConfirm(reason)}
          disabled={loading || !reason}
        >
          {loading ? "Cancelling..." : "Confirm Cancel"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
