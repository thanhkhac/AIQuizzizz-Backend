from flask import Flask, request, jsonify
import fitz  # PyMuPDF
from flasgger import Swagger, swag_from
import tempfile
import os

app = Flask(__name__)
Swagger(app)

@app.route('/outline', methods=['POST'])
@swag_from({
    'tags': ['PDF'],
    'consumes': ['multipart/form-data'],
    'parameters': [{
        'name': 'file',
        'in': 'formData',
        'type': 'file',
        'required': True,
        'description': 'PDF file'
    }],
    'responses': {
        200: {'description': 'PDF outline with nested structure'},
        400: {'description': 'Bad request'},
        500: {'description': 'Server error'}
    }
})
def get_outline():
    try:
        if 'file' not in request.files:
            return jsonify({'error': 'No file'}), 400
        
        file = request.files['file']
        if not file.filename.endswith('.pdf'):
            return jsonify({'error': 'Not a PDF'}), 400
            
        with tempfile.NamedTemporaryFile(delete=False, suffix='.pdf') as tmp:
            file.save(tmp.name)
            tmp_path = tmp.name
        
        try:
            doc = fitz.open(tmp_path)
            toc = doc.get_toc()
            doc.close()
            
            if not toc:
                return jsonify({'outline': []})
            
            # Build nested structure
            def build_nested(toc_list):
                result = []
                stack = []
                
                for level, title, page in toc_list:
                    node = {
                        'title': title.strip(),
                        'page': page,
                        'level': level,
                        'children': []
                    }
                    
                    # Remove items with higher or equal level
                    while stack and stack[-1]['level'] >= level:
                        stack.pop()
                    
                    # Add to parent or root
                    if stack:
                        stack[-1]['children'].append(node)
                    else:
                        result.append(node)
                    
                    stack.append(node)
                
                return result
            
            nested_outline = build_nested(toc)
            return jsonify({'outline': nested_outline})
            
        finally:
            os.unlink(tmp_path)
            
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True, port=1234, host='0.0.0.0')