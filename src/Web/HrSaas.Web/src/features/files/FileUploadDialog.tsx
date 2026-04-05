import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
} from "@mui/material";
import { useRef, useState } from "react";
import type { FileCategory } from "@/types/shared";
import { FILE_CATEGORIES } from "./constants";

interface FileUploadDialogProps {
  open: boolean;
  onClose: () => void;
  onUpload: (file: File, category: FileCategory, description?: string) => void;
  loading: boolean;
}

export function FileUploadDialog({
  open,
  onClose,
  onUpload,
  loading,
}: Readonly<FileUploadDialogProps>) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [category, setCategory] = useState<FileCategory>("General");
  const [description, setDescription] = useState("");

  const handleClose = () => {
    setFile(null);
    setDescription("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Upload File</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <input
            ref={fileInputRef}
            type="file"
            hidden
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
          <Button variant="outlined" onClick={() => fileInputRef.current?.click()}>
            {file ? file.name : "Choose File"}
          </Button>

          <FormControl fullWidth>
            <InputLabel>Category</InputLabel>
            <Select
              value={category}
              label="Category"
              onChange={(e) => setCategory(e.target.value as FileCategory)}
            >
              {FILE_CATEGORIES.map((c) => (
                <MenuItem key={c} value={c}>
                  {c}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <TextField
            fullWidth
            multiline
            minRows={2}
            label="Description (optional)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={() => file && onUpload(file, category, description || undefined)}
          disabled={loading || !file}
        >
          {loading ? "Uploading..." : "Upload"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
