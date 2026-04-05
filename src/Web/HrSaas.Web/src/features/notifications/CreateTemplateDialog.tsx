import { zodResolver } from "@hookform/resolvers/zod";
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
import { useForm, Controller } from "react-hook-form";
import { CATEGORIES, CHANNELS } from "./constants";
import { createTemplateSchema, type CreateTemplateForm } from "./schemas";

interface CreateTemplateDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateTemplateForm) => void;
  loading: boolean;
}

export function CreateTemplateDialog({
  open,
  onClose,
  onSubmit,
  loading,
}: Readonly<CreateTemplateDialogProps>) {
  const form = useForm<CreateTemplateForm>({
    resolver: zodResolver(createTemplateSchema),
    defaultValues: {
      name: "",
      slug: "",
      description: "",
      channel: "Email",
      category: "System",
      subjectTemplate: "",
      bodyTemplate: "",
    },
  });

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Create Notification Template</DialogTitle>
      <DialogContent>
        <Stack spacing={2} mt={1}>
          <TextField
            fullWidth
            label="Template Name"
            error={!!form.formState.errors.name}
            helperText={form.formState.errors.name?.message}
            {...form.register("name")}
          />
          <TextField
            fullWidth
            label="Slug"
            placeholder="leave-approval-email"
            error={!!form.formState.errors.slug}
            helperText={
              form.formState.errors.slug?.message ??
              "Unique identifier for programmatic access"
            }
            {...form.register("slug")}
          />
          <TextField
            fullWidth
            label="Description"
            multiline
            minRows={2}
            {...form.register("description")}
          />
          <Stack direction="row" spacing={2}>
            <Controller
              name="channel"
              control={form.control}
              render={({ field }) => (
                <FormControl fullWidth>
                  <InputLabel>Channel</InputLabel>
                  <Select {...field} label="Channel">
                    {CHANNELS.map((c) => (
                      <MenuItem key={c} value={c}>
                        {c}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
            <Controller
              name="category"
              control={form.control}
              render={({ field }) => (
                <FormControl fullWidth>
                  <InputLabel>Category</InputLabel>
                  <Select {...field} label="Category">
                    {CATEGORIES.map((c) => (
                      <MenuItem key={c} value={c}>
                        {c}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
          </Stack>
          <TextField
            fullWidth
            label="Subject Template"
            placeholder="Leave {{action}} for {{employee_name}}"
            error={!!form.formState.errors.subjectTemplate}
            helperText={
              form.formState.errors.subjectTemplate?.message ??
              "Use {{variable}} for dynamic content"
            }
            {...form.register("subjectTemplate")}
          />
          <TextField
            fullWidth
            label="Body Template"
            multiline
            minRows={4}
            placeholder="Hello {{employee_name}},\n\nYour leave request has been {{action}}."
            error={!!form.formState.errors.bodyTemplate}
            helperText={
              form.formState.errors.bodyTemplate?.message ??
              "Use {{variable}} for dynamic content"
            }
            {...form.register("bodyTemplate")}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>
        <Button
          variant="contained"
          onClick={form.handleSubmit(onSubmit)}
          disabled={loading}
        >
          {loading ? "Creating..." : "Create Template"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
