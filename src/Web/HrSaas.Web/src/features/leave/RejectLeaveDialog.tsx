import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
} from "@mui/material";
import { useState } from "react";

interface RejectLeaveDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (note: string) => void;
  loading: boolean;
}

export function RejectLeaveDialog({
  open,
  onClose,
  onConfirm,
  loading,
}: Readonly<RejectLeaveDialogProps>) {
  const [note, setNote] = useState("");

  const handleClose = () => {
    setNote("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="xs">
      <DialogTitle>Reject Leave Request</DialogTitle>
      <DialogContent>
        <TextField
          fullWidth
          multiline
          minRows={2}
          label="Rejection reason"
          value={note}
          onChange={(e) => setNote(e.target.value)}
          sx={{ mt: 1 }}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          color="error"
          disabled={!note || loading}
          onClick={() => onConfirm(note)}
        >
          Reject
        </Button>
      </DialogActions>
    </Dialog>
  );
}
