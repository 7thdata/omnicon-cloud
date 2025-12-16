function initializeTinyMce(imageUploadUrl) {
    tinymce.init({
        selector: '#myEditor',
        plugins: 'table image code link', // Added 'link' plugin
        toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | table | image | link | code', // Added 'link' button
        height: 500,
        license_key: 'gpl',
        branding: false,
        content_css: 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css,~/css/site.css',
        relative_urls: false,
        remove_script_host: false,
        convert_urls: true,
        promotion: false,
        automatic_uploads: true,
        images_upload_handler: (blobInfo, progress) => new Promise((resolve, reject) => {
            try {
                console.log('Uploading image:', blobInfo.filename());
                const formData = new FormData();
                const blob = blobInfo.blob();
                console.log('Blob size:', blob.size, 'Blob type:', blob.type);
                formData.append('file', blob, blobInfo.filename());

                const xhr = new XMLHttpRequest();
                xhr.open('POST', imageUploadUrl, true);

                xhr.upload.onprogress = (e) => {
                    if (e.lengthComputable) {
                        const progressValue = (e.loaded / e.total) * 100;
                        progress(progressValue);
                        console.log(`Upload progress: ${progressValue}%`);
                    }
                };

                xhr.onload = () => {
                    if (xhr.status < 200 || xhr.status >= 300) {
                        console.error('HTTP Error:', xhr.status);
                        reject(`HTTP Error: ${xhr.status}`);
                        return;
                    }

                    const result = JSON.parse(xhr.responseText);
                    console.log('Upload result:', result);

                    if (result.location) {
                        console.log('Image successfully uploaded. URL:', result.location);
                        resolve(result.location);
                    } else {
                        reject('Server did not return a valid "location" field.');
                    }
                };

                xhr.onerror = () => {
                    reject('Network error during image upload.');
                };

                xhr.send(formData);
            } catch (error) {
                reject('Image upload failed: ' + error.message);
            }
        })
    });
}