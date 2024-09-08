import { useState, useEffect } from 'react';
import { DataGrid } from '@mui/x-data-grid';
import { Button, IconButton, Box, Grid, Dialog, DialogActions, DialogContent, DialogTitle, Typography } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';

export default function FileGrid() {
    const [files, setFiles] = useState([]);
    const [selectedFilesCount, setSelectedFilesCount] = useState(0);
    const [selectedFiles, setSelectedFiles] = useState([]);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [emptyFiles, setEmptyFiles] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');
    const [confirmationDialogOpen, setConfirmationDialogOpen] = useState(false);
    const [fileToDelete, setFileToDelete] = useState(null);

    // API Endpoints
    const uploadFileApi = 'https://localhost:7006/api/file/upload';
    const getFilesApi = 'https://localhost:7006/api/file/getfiles';
    const deleteFileApi = 'https://localhost:7006/api/file/delete';

    // Fetch Files from API
    const fetchFiles = async () => {
        try {
            const response = await fetch(getFilesApi);
            if (response.ok) {
                const data = await response.json();
                setFiles(data);
                setErrorMessage(''); // Clear any previous error message
            } else {
                const errorData = await response.json();
                setErrorMessage(`Error fetching files: ${errorData.message || response.statusText}`);
                setFiles([]);
            }
        } catch (error) {
            setErrorMessage('An unexpected error occurred while fetching files.');
            setFiles([]);
        }
    };

    // Handle File Upload
    const handleFileUpload = (event) => {
        const fileList = event.target.files;
        const txtFiles = Array.from(fileList).filter(file => file.type === 'text/plain');
        const nonEmptyFiles = txtFiles.filter(file => file.size > 0);
        const emptyFilesList = txtFiles.filter(file => file.size === 0);

        if (emptyFilesList.length > 0) {
            setEmptyFiles(emptyFilesList.map(file => file.name));
        }

        if (nonEmptyFiles.length === 0) {
            setErrorMessage('Please select at least one .txt file that is not empty.');
            return;
        }

        setSelectedFilesCount(nonEmptyFiles.length);
        setSelectedFiles(nonEmptyFiles);
        setDialogOpen(true);
        setErrorMessage(''); // Clear any previous error message
    };

    // Confirm File Upload
    const handleConfirmUpload = async () => {
        const formData = new FormData();
        selectedFiles.forEach(file => formData.append('files', file));

        try {
            const response = await fetch(uploadFileApi, {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                fetchFiles(); // Reload the file list
                setSelectedFilesCount(0); // Reset the file count after upload
                setSelectedFiles([]);
                setEmptyFiles([]);
                setDialogOpen(false); // Close the dialog
                setErrorMessage(''); // Clear any previous error message
            } else {
                const errorData = await response.text();
                setErrorMessage(`Error uploading file: ${errorData}`);
            }
        } catch (error) {
            setErrorMessage('An unexpected error occurred while uploading files.');
        }
        finally {
            setDialogOpen(false); // Close the dialog whether successful or not
        }
    };

    // Open Confirmation Dialog for Deletion
    const openDeleteConfirmationDialog = (file) => {
        setFileToDelete(file);
        setConfirmationDialogOpen(true);
    };

    // Confirm File Deletion
    const handleConfirmDelete = async () => {
        if (fileToDelete) {
            try {
                const response = await fetch(`${deleteFileApi}?key=${fileToDelete.id}`, {
                    method: 'DELETE',
                });

                if (response.ok) {
                    fetchFiles(); // Reload the file list
                    setErrorMessage(''); // Clear any previous error message
                } else {
                    const errorData = await response.json();
                    setErrorMessage(`Error deleting file: ${errorData.message || response.statusText}`);
                }
            } catch (error) {
                setErrorMessage('An unexpected error occurred while deleting the file.');
            }
        }
        setConfirmationDialogOpen(false); // Close the dialog whether successful or not
    };

    // Cancel File Deletion
    const handleCancelDelete = () => {
        setFileToDelete(null);
        setConfirmationDialogOpen(false);
    };

    useEffect(() => {
        fetchFiles();
    }, []);

    // File columns for the DataGrid
    const columns = [
        { field: 'id', headerName: 'ID', width: 70 },
        { field: 'name', headerName: 'File Name', flex: 1 },
        { field: 'extension', headerName: 'Extension', flex: 1 },
        { field: 'createdOn', headerName: 'Created On', flex: 1 },
        {
            field: 'actions',
            headerName: 'Actions',
            renderCell: (params) => (
                <IconButton
                    color="secondary"
                    onClick={() => openDeleteConfirmationDialog(params.row)}
                >
                    <DeleteIcon />
                </IconButton>
            ),
            sortable: false,
            width: 100,
        },
    ];

    return (
        <Box sx={{ width: '100%', height: 400 }}>
            {/* Display Error Message */}
            {errorMessage && (
                <Typography color="error" variant="body1" sx={{ mb: 2 }}>
                    {errorMessage}
                </Typography>
            )}

            <Grid container justifyContent="flex-end" sx={{ mb: 2 }}>
                <input
                    type="file"
                    multiple
                    accept=".txt" // Only allows .txt files
                    onChange={handleFileUpload}
                    style={{ display: 'none' }}
                    id="file-upload"
                />
                <label htmlFor="file-upload">
                    <Button variant="contained" component="span">
                        Upload Files ({selectedFilesCount})
                    </Button>
                </label>
            </Grid>

            <DataGrid
                rows={files}
                columns={columns}
                pageSize={5}
                getRowId={(row) => row.id}
                components={{
                    NoRowsOverlay: () => <div style={{ padding: 16 }}>No files available</div>,
                }}
            />

            {/* Confirmation Dialog */}
            <Dialog
                open={dialogOpen}
                onClose={() => setDialogOpen(false)}
            >
                <DialogTitle>Confirm File Upload</DialogTitle>
                <DialogContent>
                    {emptyFiles.length > 0 && (
                        <Typography color="error">
                            The following files are empty and will not be uploaded:
                            <ul>
                                {emptyFiles.map(file => <li key={file}>{file}</li>)}
                            </ul>
                        </Typography>
                    )}
                    <Typography>
                        Are you sure you want to upload the selected files?
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDialogOpen(false)} color="primary">
                        Cancel
                    </Button>
                    <Button onClick={handleConfirmUpload} color="primary">
                        Upload
                    </Button>
                </DialogActions>
            </Dialog>
            {/* Delete Confirmation Dialog */}
            <Dialog
                open={confirmationDialogOpen}
                onClose={handleCancelDelete}
            >
                <DialogTitle>Confirm File Deletion</DialogTitle>
                <DialogContent>
                    <Typography>
                        Are you sure you want to delete the file '{fileToDelete?.name}'?
                    </Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleCancelDelete} color="primary">
                        Cancel
                    </Button>
                    <Button onClick={handleConfirmDelete} color="primary">
                        Delete
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
}
