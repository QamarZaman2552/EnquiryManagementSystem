const fs = require('fs');
const content = fs.readFileSync('src/app/pages/new-enquiry/new-enquiry.html', 'utf8');

const styleMatch = content.match(/<style>([\s\S]*?)<\/style>/);
if(styleMatch) {
    fs.writeFileSync('src/app/pages/new-enquiry/new-enquiry.css', styleMatch[1]);
}

const bodyMatch = content.match(/<body>([\s\S]*?)<script/);
if(bodyMatch) {
    let body = bodyMatch[1].trim();
    // Use proper single quotes and template strings are ok here.
    body = body.replace(/onclick="selectService\(this, (\d+), '[^']+'\)"/g, `(click)="formData.serviceId = $1"`);
    body = body.replace(/id="charCount">0</g, `id="charCount">{{ formData.message.length }}<`);
    // also remove the toast container because it breaks without the JS
    body = body.replace(/<div class="toast-success" id="successToast">[\s\S]*?<\/div>\s*<\/div>\s*<\/div>/g, '');
    fs.writeFileSync('src/app/pages/new-enquiry/new-enquiry.html', body);
}
console.log('Done');
